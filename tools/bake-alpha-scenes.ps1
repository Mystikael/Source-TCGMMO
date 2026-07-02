# RETIRED: alpha scenes are authored only via Unity Editor API (AlphaSceneSetup.BatchSetup).
# Use tools/compile-unity.ps1 for the authoritative scene + build-settings gate.
Write-Error 'bake-alpha-scenes.ps1 is retired. Run: powershell -File tools/compile-unity.ps1'
exit 1