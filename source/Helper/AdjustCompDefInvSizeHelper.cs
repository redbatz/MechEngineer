﻿using System;
using BattleTech;

namespace MechEngineer
{
    internal class AdjustCompDefInvSizeHelper
    {
        private readonly IIdentifier identifier;
        private readonly ValueChange<int> change;

        internal AdjustCompDefInvSizeHelper(IIdentifier identifier, ValueChange<int> change)
        {
            this.identifier = identifier;
            this.change = change;
        }

        internal void AdjustComponentDef(MechComponentDef def)
        {
            if (!identifier.IsCustomType(def))
            {
                return;
            }

            var newSize = change.Change(def.InventorySize);
            if (newSize < 1)
            {
                return;
            }

            var value = newSize;
            var propInfo = typeof(UpgradeDef).GetProperty("InventorySize");
            var propValue = Convert.ChangeType(value, propInfo.PropertyType);
            propInfo.SetValue(def, propValue, null);
        }
    }
}