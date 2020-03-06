# Universal Fermenter
This mod contains the "universal fermenter" - as user this is pretty much irrelevant for you. The fermenting barrel gets replaced by a universal one which can ferment different things, depending on it's setting. However, this means if you add this to an existing save in which you have fermenting barrels, they might disappear - either way, you will need to reconstruct them, as your old ones will not be automatically replaced by the universal ones.

Originally Universal Fermenter is made by Kubouch

Updated to 1.0 by Syrchalis

## Modders using this mod
In order to use this mod, you'll need to patch your recipie into the universal fermenter. An example of the patch operation used by the Blueberries mod looks like this:

<Operation Class="PatchOperationAdd">
		<xpath>Defs/ThingDef[defName = "UniversalFermenter"]/comps/li[@Class="UniversalFermenter.CompProperties_UniversalFermenter"]/products</xpath>
		  <value>
			<li>
				<thingDef>BlueberryWine</thingDef>
				<ingredientFilter>
					<thingDefs>
						<li>Mash</li>
					</thingDefs>
				</ingredientFilter>
				<baseFermentationDuration>900000</baseFermentationDuration>
			</li>
		  </value>
	</Operation>