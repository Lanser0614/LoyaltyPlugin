# SDK verification TODO

## Release blockers
- [ ] Replace placeholder plugin license GUID in `PluginLicenseModuleId` after obtaining a real iiko developer license.

## SDK integration checks
- [ ] Verify button/menu registration in iikoFront shell.
- [ ] Verify current order access and cashier extraction from `PluginContext.Operations`.
- [ ] Verify discount type resolution for loyalty discount.
- [ ] Verify exact `AddFlexibleSumDiscount` signature.
- [ ] Verify exact `ChangeSelectiveDiscount` signature.
- [ ] Verify product/free item adding flow in edit session.
- [ ] Verify saving `application_id` to order comment/custom data.
- [ ] Verify focus/window ownership workaround for WPF window in iikoFront.

## Definition of done
- [ ] For each item above, add a short verification note with SDK/API version and the tested scenario.
