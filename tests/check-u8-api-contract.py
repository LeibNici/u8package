#!/usr/bin/env python3
"""Static checks for the public U8 Bridge REST route contract."""

from pathlib import Path
import csv
import re
import sys


ROOT = Path(__file__).resolve().parents[1]
CONTROLLERS = ROOT / "src" / "Xinchuan.U8Bridge" / "Controllers"
OPENAPI = ROOT / "docs" / "u8-bridge-openapi.yaml"
CATALOG = ROOT / "docs" / "u8-official-api-catalog.csv"
DOCS = [
    ROOT / "README.md",
    ROOT / "docs" / "README.md",
    ROOT / "docs" / "u8-bridge-api.md",
]
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


def assert_official_route_is_deprecated():
    source = read(CONTROLLERS / "U8Controller.Official.cs")
    if '[Route("official/{*apiPath}")]' not in source:
        fail("Generic official compatibility route was removed.")
    if "Obsolete" not in source or "internal compatibility" not in source:
        fail("Generic official route must be marked obsolete/internal compatibility.")

    openapi = read(OPENAPI)
    official_marker = "# BEGIN GENERATED OFFICIAL U8 API PATHS"
    if official_marker not in openapi:
        fail("OpenAPI official generated block marker is missing.")
    official_block = openapi.split(official_marker, 1)[1]
    if "deprecated: true" not in official_block:
        fail("OpenAPI official operations must be deprecated.")
    if "Internal compatibility endpoint" not in official_block:
        fail("OpenAPI official descriptions must say internal compatibility endpoint.")


def assert_openapi_official_catalog_contract():
    openapi = read(OPENAPI)
    top_level_tags = parse_top_level_tags(openapi)
    official_blocks = parse_official_path_blocks(openapi)
    official_paths = sorted(official_blocks.keys())
    catalog_paths = sorted("/api/u8/official/" + address for address in unique_catalog_addresses())

    if official_paths != catalog_paths:
        missing = sorted(set(catalog_paths) - set(official_paths))
        extra = sorted(set(official_paths) - set(catalog_paths))
        fail("OpenAPI official paths differ from catalog. Missing: "
             + first_or_none(missing) + "; extra: " + first_or_none(extra))

    operation_ids = re.findall(r"(?m)^\s+operationId:\s+([A-Za-z0-9_]+)\s*$", openapi)
    duplicates = sorted(name for name in set(operation_ids) if operation_ids.count(name) > 1)
    if duplicates:
        fail("OpenAPI operationId must be unique: " + duplicates[0])

    for path, block in official_blocks.items():
        assert_official_operation_block(path, block, top_level_tags)


def assert_docs_recommend_strong_typed_paths():
    for path in DOCS:
        content = read(path)
        if "/api/u8/<business-resource>/<action>" not in content:
            fail(f"{path.relative_to(ROOT)} must document the recommended route style.")
        if "/api/u8/sales-order/save" not in content:
            fail(f"{path.relative_to(ROOT)} must show sales-order/save as the recommended example.")

    openapi = read(OPENAPI)
    if "/api/u8/sales-order/save:" not in openapi:
        fail("OpenAPI must keep the strong typed sales-order route.")
    if "Recommended strong typed Bridge endpoint" not in openapi:
        fail("OpenAPI must mark the sales-order route as recommended.")


def parse_top_level_tags(openapi):
    before_paths = openapi.split("\npaths:", 1)[0]
    if "\ntags:" not in before_paths:
        fail("OpenAPI top-level tags section is missing.")

    tag_section = before_paths.split("\ntags:", 1)[1]
    return set(
        unquote(match.group(1).strip())
        for match in re.finditer(r"(?m)^\s+- name:\s+(.+?)\s*$", tag_section))


def parse_official_path_blocks(openapi):
    if OFFICIAL_BEGIN not in openapi or OFFICIAL_END not in openapi:
        fail("OpenAPI generated official path markers are missing.")

    official_block = openapi.split(OFFICIAL_BEGIN, 1)[1].split(OFFICIAL_END, 1)[0]
    blocks = {}
    path_pattern = re.compile(r'(?m)^  "(/api/u8/official/[^"]+)":\s*$')
    matches = list(path_pattern.finditer(official_block))
    for index, match in enumerate(matches):
        start = match.end()
        end = matches[index + 1].start() if index + 1 < len(matches) else len(official_block)
        blocks[match.group(1)] = official_block[start:end]
    return blocks


def unique_catalog_addresses():
    with CATALOG.open(encoding="utf-8-sig", newline="") as handle:
        rows = csv.DictReader(handle)
        return sorted({
            row["api_address"].strip()
            for row in rows
            if row.get("api_address", "").strip()
        })


def assert_official_operation_block(path, block, top_level_tags):
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
    if bridge_status != "generic-official":
        fail(path + " x-bridge-status must stay generic-official.")
    if "deprecated: true" not in block:
        fail(path + " must be deprecated.")
    if "Internal compatibility endpoint" not in block:
        fail(path + " must describe itself as an internal compatibility endpoint.")


def required_value(path, block, field):
    match = re.search(r"(?m)^\s+" + re.escape(field) + r":\s+(.+?)\s*$", block)
    if not match:
        fail(path + " missing " + field + ".")
    return match.group(1).strip()


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
    assert_official_route_is_deprecated()
    assert_openapi_official_catalog_contract()
    assert_docs_recommend_strong_typed_paths()
    print("U8 Bridge API route contract checks passed.")


if __name__ == "__main__":
    main()
