# U8 API DLLs

This directory contains the official U8API sample DLL dependencies used by the
Bridge official adapter.

GitHub Actions copies `*.dll` from this directory into the release package root,
next to `Xinchuan.U8Bridge.exe`, so the runtime can resolve `U8EnvContext`,
`U8ApiAddress`, and `U8ApiBroker` without a separate DLL path setting.

If the customer U8 environment requires version-specific DLLs, replace these
files with the matching official DLLs before packaging.
