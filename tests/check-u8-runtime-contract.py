#!/usr/bin/env python3
"""Static checks for the U8 Bridge process runtime environment."""

from pathlib import Path
import sys
import xml.etree.ElementTree as ET


ROOT = Path(__file__).resolve().parents[1]
CSPROJ = ROOT / "src" / "Xinchuan.U8Bridge" / "Xinchuan.U8Bridge.csproj"


def fail(message):
    print("FAIL: " + message)
    sys.exit(1)


def read(path):
    return path.read_text(encoding="utf-8")


def assert_runtime_environment_contract():
    source_file = ROOT / "src" / "Xinchuan.U8Bridge" / "Services" / "BridgeRuntimeEnvironment.cs"
    if not source_file.exists():
        fail("BridgeRuntimeEnvironment.cs must exist.")

    includes = csproj_compile_includes()
    required_includes = [
        "Services\\BridgeRuntimeEnvironment.cs",
        "U8\\OfficialU8ApiAliasMap.Generated.cs",
    ]
    for include in required_includes:
        if include not in includes:
            fail(include + " must be included in Xinchuan.U8Bridge.csproj.")

    program = read(ROOT / "src" / "Xinchuan.U8Bridge" / "Program.cs")
    required_startup_calls = [
        "BridgeRuntimeEnvironment.InitializeForProcess();",
        "BridgeRuntimeEnvironment.AssertInitializedForMapperSnapshot();",
    ]
    for call in required_startup_calls:
        if call not in program:
            fail("Program.cs is missing runtime environment call: " + call)

    runtime = read(ROOT / "src" / "Xinchuan.U8Bridge" / "U8" / "U8BrokerRuntime.cs")
    if "BridgeRuntimeEnvironment.EnterBrokerInvocationScope()" not in runtime:
        fail("U8BrokerRuntime must guard broker invocation current directory.")

    tests_readme = read(ROOT / "tests" / "README.md")
    for expected in ["Release 46", "TEMP", "TMP", "Environment.CurrentDirectory"]:
        if expected not in tests_readme:
            fail("tests/README.md must document runtime environment verification: " + expected)


def assert_material_app_required_empty_fields_are_preserved():
    mapper = read(ROOT / "src" / "Xinchuan.U8Bridge" / "U8" / "U8AdvancedDocumentMapper.cs")
    required_snippets = [
        'head.Rows[0]["id"] = string.Empty;',
        'row["autoid"] = string.Empty;',
    ]
    for snippet in required_snippets:
        if snippet not in mapper:
            fail("materialapp/Add must explicitly assign required blank BO key: " + snippet)

    forbidden_snippets = [
        'Put(head, "id", string.Empty',
        'Put(row, "autoid", string.Empty',
    ]
    for snippet in forbidden_snippets:
        if snippet in mapper:
            fail("materialapp/Add required blank BO key cannot use Put(), which skips empty strings: " + snippet)


def csproj_compile_includes():
    namespace = {"msb": "http://schemas.microsoft.com/developer/msbuild/2003"}
    try:
        root = ET.parse(CSPROJ).getroot()
    except ET.ParseError as ex:
        fail("Xinchuan.U8Bridge.csproj is not valid XML: " + str(ex))

    return {
        element.attrib.get("Include", "")
        for element in root.findall(".//msb:Compile", namespace)
    }


def main():
    assert_runtime_environment_contract()
    assert_material_app_required_empty_fields_are_preserved()
    print("U8 Bridge runtime contract checks passed.")


if __name__ == "__main__":
    main()
