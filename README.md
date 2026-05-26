# Bellissimo iikoFront Loyalty Plugin (MVP)

## Setup
1. Install **.NET Framework 4.7.2 Developer Pack**.
2. Add SDK reference to `Resto.Front.Api.Vx.dll` (update HintPath in csproj as needed).
3. Configure `src/Bellissimo.IikoFront.LoyaltyPlugin/app.config`:
   - `ApiBaseUrl`
   - `BasicAuthLogin`
   - `BasicAuthPassword`
   - terminal-group-specific settings (`BranchId`, `TerminalGroupId`, `PosId`)
4. Insert real `PluginLicenseModuleId` in `BellissimoLoyaltyPlugin.cs`.
5. Build project (`msbuild` or Visual Studio).
6. Copy output files (`.dll`, `Manifest.xml`, config) to iikoFront `Plugins` folder.
7. Start iikoFront with developer license.

## Notes
- Loyalty service is source of truth for rewards/stacking rules.
- Plugin does not contain reward business logic.
- API errors are mapped to cashier-friendly Russian messages.
