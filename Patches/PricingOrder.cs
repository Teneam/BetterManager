using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BetterManager.Patches
{

    [HarmonyPatch]
    internal class PricingOrder
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerNetwork), nameof(PlayerNetwork.PriceSetFromNumpad))]
        private static void AddPrincingOrder(PlayerNetwork __instance, int productID)
        {
            if (!Input.GetKeyDown(KeyCode.E))
            {
                return;
            }
            //Plugin.Logger.LogInfo(string.Format("Product {0} Added to list", productID));

            ManagerBlackboard managerBlackboard = UnityEngine.Object.FindFirstObjectByType<ManagerBlackboard>();

            // Get Product details
            Data_Product product = ProductListing.Instance.productPrefabs[productID].GetComponent<Data_Product>();
            var productName = product.productBrand;

            // Calculate box price using unit cost, inflation, and items per box
            float inflation = ProductListing.Instance.tierInflation[product.productTier];
            float basePricePerUnit = product.basePricePerUnit * inflation;
            basePricePerUnit = Mathf.Round(basePricePerUnit * 100f) / 100f;
            float boxPrice = basePricePerUnit * product.maxItemsPerBox;
            boxPrice = Mathf.Round(boxPrice * 100f) / 100f;

            // Add new item into the shopping list
            managerBlackboard.AddShoppingListProduct(productID, boxPrice);

            //Notification something has happened
            GameData.Instance.PlayPopSound();

            //Plugin.Logger.LogInfo(string.Format("Added ProductID {0}, Product {1}, Price {2}", productID, productName, boxPrice));

        }
    }
}
