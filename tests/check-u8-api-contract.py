#!/usr/bin/env python3
"""Static checks for the public U8 Bridge REST route contract."""

from pathlib import Path
import csv
import re
import subprocess
import sys


ROOT = Path(__file__).resolve().parents[1]
CONTROLLERS = ROOT / "src" / "Xinchuan.U8Bridge" / "Controllers"
OPENAPI = ROOT / "docs" / "u8-bridge-openapi.yaml"
CATALOG = ROOT / "docs" / "u8-official-api-catalog.csv"
DOCS = [ROOT / "README.md", ROOT / "docs" / "README.md", ROOT / "docs" / "u8-bridge-api.md"]
OFFICIAL_BEGIN = "  # BEGIN GENERATED OFFICIAL U8 API PATHS"
OFFICIAL_END = "  # END GENERATED OFFICIAL U8 API PATHS"


def fail(message):
    print("FAIL: " + message)
    sys.exit(1)


def read(path):
    return path.read_text(encoding="utf-8")


def assert_controller_routes_under_api_u8():
    controller = read(CONTROLLERS / "U8Controller.cs")
    if '[RoutePrefix("api/u8")]' not in controller:
        fail("U8Controller route prefix must stay api/u8.")

    route_pattern = re.compile(r'\[Route\("([^"]+)"\)\]')
    for path in sorted(CONTROLLERS.glob("U8Controller*.cs")):
        for route in route_pattern.findall(read(path)):
            if route.startswith("/") or route.startswith("api/"):
                fail(f"{path.name} route must be relative to api/u8: {route}")
            full_route = "/api/u8/" + route.replace("{*apiPath}", "{apiPath}")
            if not full_route.startswith("/api/u8/"):
                fail(f"{path.name} route escaped /api/u8: {route}")
            if "/officail/" in full_route or "/official/U8API/" in full_route:
                fail(f"{path.name} exposes a concrete official U8API path: {route}")


def assert_legacy_official_route_is_internal():
    source = read(CONTROLLERS / "U8Controller.Official.cs")
    if '[Route("official/{*apiPath}")]' not in source:
        fail("Generic official compatibility route was removed.")
    if "Obsolete" not in source or "internal compatibility" not in source:
        fail("Generic official route must be marked obsolete/internal compatibility.")
    if '[Route("{*bridgePath}")]' not in source:
        fail("Migrated official alias catch-all route is missing.")
    if "OfficialU8ApiAliasMap.TryGetOfficialApi" not in source:
        fail("Migrated official alias route must resolve through OfficialU8ApiAliasMap.")

    openapi = read(OPENAPI)
    if re.search(r'(?m)^  "/api/u8/official/U8API/', openapi):
        fail("OpenAPI paths must not expose /api/u8/official/U8API/... as generated paths.")


def assert_openapi_yaml_parses():
    result = subprocess.run(
        ["ruby", "-e", "require 'yaml'; YAML.load_file(ARGV[0])", str(OPENAPI)],
        cwd=str(ROOT),
        text=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        check=False)
    if result.returncode != 0:
        fail("OpenAPI YAML is not parseable: " + result.stderr.strip())


def assert_openapi_official_catalog_contract():
    openapi = read(OPENAPI)
    top_level_tags = parse_top_level_tags(openapi)
    alias_blocks = parse_generated_alias_blocks(openapi)
    alias_paths = sorted(alias_blocks.keys())
    expected_aliases = generated_catalog_aliases()
    expected_paths = sorted(expected_aliases.keys())

    if alias_paths != expected_paths:
        missing = sorted(set(expected_paths) - set(alias_paths))
        extra = sorted(set(alias_paths) - set(expected_paths))
        fail("OpenAPI generated alias paths differ from catalog. Missing: "
             + first_or_none(missing) + "; extra: " + first_or_none(extra))

    operation_ids = re.findall(r"(?m)^\s+operationId:\s+([A-Za-z0-9_]+)\s*$", openapi)
    duplicates = sorted(name for name in set(operation_ids) if operation_ids.count(name) > 1)
    if duplicates:
        fail("OpenAPI operationId must be unique: " + duplicates[0])

    for path, block in alias_blocks.items():
        assert_official_alias_operation_block(path, block, top_level_tags, expected_aliases[path])

    assert_alias_map_contract(expected_aliases)


def assert_docs_recommend_strong_typed_paths():
    for path in DOCS:
        content = read(path)
        if "/api/u8/<business-resource>/<action>" not in content:
            fail(f"{path.relative_to(ROOT)} must document the recommended route style.")
        if "/api/u8/sales-order/save" not in content:
            fail(f"{path.relative_to(ROOT)} must show sales-order/save as the recommended example.")
        if "/api/u8/ap-apply-pay/cancel-sign" not in content:
            fail(f"{path.relative_to(ROOT)} must show a migrated official alias example.")
        if "/api/u8/official/{官方地址}" not in content:
            fail(f"{path.relative_to(ROOT)} must document the legacy official compatibility route.")

    openapi = read(OPENAPI)
    if "/api/u8/sales-order/save:" not in openapi:
        fail("OpenAPI must keep the strong typed sales-order route.")
    if "Recommended strong typed Bridge endpoint" not in openapi:
        fail("OpenAPI must mark the sales-order route as recommended.")
    if "Migrated official U8 API Bridge endpoint" not in openapi:
        fail("OpenAPI must describe generated aliases as migrated Bridge endpoints.")
    if openapi.count("Legacy /api/u8/official/") > 1:
        fail("OpenAPI must not repeat the legacy official compatibility note in generated operations.")


def assert_openapi_strong_typed_schema_contract():
    openapi = read(OPENAPI)
    purchase_order = schema_block(openapi, "PurchaseOrderConfirmRequest", "InboundAddRequest")
    if "businessType:" not in purchase_order or "default: 普通采购" not in purchase_order:
        fail("OpenAPI PurchaseOrderConfirmRequest must document businessType default.")

    consignment = schema_block(openapi, "ConsignmentSaveRequest", "SaleOutAddRequest")
    for expected in ["currency:", "exchangeRate:", "taxRate:"]:
        if expected not in consignment:
            fail("OpenAPI ConsignmentSaveRequest is missing: " + expected)

    outbound_item = schema_block(openapi, "OutboundItem", "MaterialOutItem")
    if "unitCode:" not in outbound_item:
        fail("OpenAPI OutboundItem must document unitCode.")

    material_out_item = schema_block(openapi, "MaterialOutItem", "MaterialAppItem")
    if "unitCode:" not in material_out_item:
        fail("OpenAPI MaterialOutItem must document unitCode.")


def schema_block(openapi, name, next_name):
    start_marker = "    " + name + ":"
    end_marker = "    " + next_name + ":"
    if start_marker not in openapi or end_marker not in openapi:
        fail("OpenAPI schema markers are missing for " + name)
    return openapi.split(start_marker, 1)[1].split(end_marker, 1)[0]


def parse_top_level_tags(openapi):
    before_paths = openapi.split("\npaths:", 1)[0]
    if "\ntags:" not in before_paths:
        fail("OpenAPI top-level tags section is missing.")

    tag_section = before_paths.split("\ntags:", 1)[1]
    return set(
        unquote(match.group(1).strip())
        for match in re.finditer(r"(?m)^\s+- name:\s+(.+?)\s*$", tag_section))


def parse_generated_alias_blocks(openapi):
    if OFFICIAL_BEGIN not in openapi or OFFICIAL_END not in openapi:
        fail("OpenAPI generated official alias path markers are missing.")

    official_block = openapi.split(OFFICIAL_BEGIN, 1)[1].split(OFFICIAL_END, 1)[0]
    blocks = {}
    path_pattern = re.compile(r'(?m)^  "(/api/u8/[^"]+)":\s*$')
    matches = list(path_pattern.finditer(official_block))
    for index, match in enumerate(matches):
        start = match.end()
        end = matches[index + 1].start() if index + 1 < len(matches) else len(official_block)
        if match.group(1).startswith("/api/u8/official/"):
            fail("Generated alias block must not include legacy official path: " + match.group(1))
        blocks[match.group(1)] = official_block[start:end]
    return blocks


def catalog_rows_by_address():
    rows_by_address = {}
    with CATALOG.open(encoding="utf-8-sig", newline="") as handle:
        for row in csv.DictReader(handle):
            address = row.get("api_address", "").strip()
            if address and address not in rows_by_address:
                rows_by_address[address] = row
    return rows_by_address


def generated_catalog_aliases():
    strong_typed_addresses = strong_typed_official_addresses()
    aliases = {}
    for address in catalog_rows_by_address():
        if address in strong_typed_addresses:
            continue
        alias = official_api_alias(address)
        if alias in aliases:
            fail("Generated official alias is not unique: " + alias)
        aliases[alias] = address
    return aliases


def strong_typed_official_addresses():
    before_generated = read(OPENAPI).split(OFFICIAL_BEGIN, 1)[0]
    addresses = set()
    for block in operation_blocks(before_generated).values():
        status = unquote(optional_value(block, "x-bridge-status"))
        address = unquote(optional_value(block, "x-u8-official-api"))
        if address and status != "unsupported-shell":
            addresses.add(address)
    return addresses


def operation_blocks(openapi_fragment):
    blocks = {}
    path_pattern = re.compile(r"(?m)^  (/api/u8/[^:]+):\s*$")
    matches = list(path_pattern.finditer(openapi_fragment))
    for index, match in enumerate(matches):
        start = match.end()
        end = matches[index + 1].start() if index + 1 < len(matches) else len(openapi_fragment)
        blocks[match.group(1)] = openapi_fragment[start:end]
    return blocks


def official_api_alias(address):
    if address.startswith("U8API/"):
        parts = address.split("/")[1:]
    elif address.startswith("U8ERP_"):
        parts = address.split("/")[1:] or [address]
    else:
        parts = address.split("/")
    if len(parts) == 1:
        parts.append("invoke")
    return "/api/u8/" + "/".join(filter(None, [slugify(part) for part in parts]))


def slugify(value):
    value = re.sub(r"([a-z0-9])([A-Z])", r"\1-\2", value)
    value = re.sub(r"([A-Z]+)([A-Z][a-z])", r"\1-\2", value)
    value = re.sub(r"[^A-Za-z0-9]+", "-", value)
    return re.sub(r"-+", "-", value).strip("-").lower()


def assert_alias_map_contract(expected_paths):
    source = read(ROOT / "src" / "Xinchuan.U8Bridge" / "U8" / "OfficialU8ApiAliasMap.Generated.cs")
    pairs = dict(
        ("/api/u8/" + bridge_path, official_api)
        for bridge_path, official_api in re.findall(r'Pair\("([^"]+)",\s+"([^"]+)"\)', source))
    if pairs != expected_paths:
        missing = sorted(set(expected_paths) - set(pairs))
        extra = sorted(set(pairs) - set(expected_paths))
        fail("C# alias map differs from generated catalog aliases. Missing: "
             + first_or_none(missing) + "; extra: " + first_or_none(extra))


def assert_official_alias_operation_block(path, block, top_level_tags, expected_address):
    tags = parse_inline_list(required_value(path, block, "tags"))
    if len(tags) != 1:
        fail(path + " must have exactly one Swagger tag.")

    category = unquote(required_value(path, block, "x-u8-category"))
    document = unquote(required_value(path, block, "x-u8-document"))
    expected_tag = document or category or "官方U8 API"
    if tags[0] != expected_tag:
        fail(path + " tag must be x-u8-document || x-u8-category || 官方U8 API.")
    if expected_tag != "官方U8 API" and tags[0] == "官方U8 API":
        fail(path + " must not use 官方U8 API when metadata has a better tag.")
    if tags[0] not in top_level_tags:
        fail(path + " tag is missing from top-level OpenAPI tags: " + tags[0])

    required_value(path, block, "summary")
    bridge_status = unquote(required_value(path, block, "x-bridge-status"))
    if bridge_status != "migrated-official-alias":
        fail(path + " x-bridge-status must stay migrated-official-alias.")
    official_api = unquote(required_value(path, block, "x-u8-official-api"))
    if official_api != expected_address:
        fail(path + " x-u8-official-api does not match catalog: " + official_api)
    if "deprecated: true" in block:
        fail(path + " must not be marked deprecated; legacy /api/u8/official is the deprecated route.")
    if "Internal compatibility endpoint" in block:
        fail(path + " must not describe the migrated alias as an internal compatibility endpoint.")
    if "Legacy /api/u8/official/" in block:
        fail(path + " must not repeat the legacy official compatibility route in the operation description.")
    if "Migrated official U8 API Bridge endpoint" not in block:
        fail(path + " must describe itself as a migrated Bridge endpoint.")


def required_value(path, block, field):
    match = re.search(r"(?m)^\s+" + re.escape(field) + r":\s+(.+?)\s*$", block)
    if not match:
        fail(path + " missing " + field + ".")
    return match.group(1).strip()


def optional_value(block, field):
    match = re.search(r"(?m)^\s+" + re.escape(field) + r":\s+(.+?)\s*$", block)
    return match.group(1).strip() if match else ""


def parse_inline_list(value):
    if not value.startswith("[") or not value.endswith("]"):
        fail("Only inline one-line tag lists are supported by this static checker: " + value)
    raw = value[1:-1].strip()
    if not raw:
        return []
    return [unquote(item.strip()) for item in raw.split(",")]


def unquote(value):
    if len(value) >= 2 and value[0] == '"' and value[-1] == '"':
        return value[1:-1]
    return value

def first_or_none(values):
    return values[0] if values else "none"


def main():
    assert_controller_routes_under_api_u8()
    assert_legacy_official_route_is_internal()
    assert_openapi_yaml_parses()
    assert_openapi_official_catalog_contract()
    assert_openapi_strong_typed_schema_contract()
    assert_docs_recommend_strong_typed_paths()
    print("U8 Bridge API route contract checks passed.")


if __name__ == "__main__":
    main()
