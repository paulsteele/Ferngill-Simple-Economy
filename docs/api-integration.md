See https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Integrations for more information.

1. Copy `FerngillSimpleEconomy/api/IFerngillSimpleEconomyApi.cs` into your project.
2. Grab the object from SMAPI:
```csharp
var fseApi = this.Helper.ModRegistry.GetApi<IFerngillSimpleEconomyApi>("paulsteele.fse");

if (fseApi == null)
{
	return;
}
```
3. Use the API:

```csharp
fseApi.AdjustSupply(new Object("(O)72", 100)); // Increase supply of item ID 72 (Diamond) by 100
```