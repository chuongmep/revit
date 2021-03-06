﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using AMEEClient.MaterialMapper;
using Autodesk.Revit.DB;
using log4net;

namespace AMEE_in_Revit.Addin.CO2eParameter
{
    public class ElementCO2eCalculator
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ElementCO2eCalculator));
        private MaterialMapper _materialMapper;

        public ElementCO2eCalculator(MaterialMapper materialMapper)
        {
            _materialMapper = materialMapper;
        }

        public void UpdateElementCO2eParameters(ICollection<Element> forElements)
        {
            foreach (var element in forElements)
            {
                var materialKey = "undefined";
                try
                {
                    if (!element.ParametersMap.Contains("CO2e")) continue;

                    double totalElementCO2e = 0;
                    ICollection<ElementId> materialSetIterator = element.GetMaterialIds(false);
                    foreach (ElementId id in materialSetIterator)
                    {
                        Element material = element.Document.GetElement(id);
                        var volumeInM3 = element.GetMaterialVolume(id);
                        materialKey = material.Id + ":" + material.Name;

                        var ameeMaterial = _materialMapper.GetMaterialDataItem(materialKey);
                        totalElementCO2e += ameeMaterial.CalculateCO2eByVolume(volumeInM3);
                    }
                    element.ParametersMap.get_Item("CO2e").Set(totalElementCO2e);
                }
                catch (Exception e)
                {
                    logger.DebugFormat("Unable to add CO2e value for element {0} -> material: {1} because {2}",
                        element.Name, materialKey, e);
                    element.ParametersMap.get_Item("CO2e").Set(0);
                }
            }
        }
    }
}
