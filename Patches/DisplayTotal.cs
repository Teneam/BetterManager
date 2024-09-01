using HarmonyLib;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;
using UnityEngine.TextCore.Text;
using HutongGames.PlayMaker.Actions;
using System.Xml.Linq;
using System.Security.Policy;

namespace BetterManager.Patches
{
    [HarmonyPatch]
    internal class DisplayTotal
    {
        private static int fixedCounter;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ManagerBlackboard), nameof(ManagerBlackboard.FixedUpdate))]
        private static void AddTotals(ManagerBlackboard __instance)
        {
            if (!__instance.tabsOBJ.activeSelf || __instance.shopItemsParent.transform.childCount <= 0)
            {
                return;
            }
            if (fixedCounter == 0)
            {
                foreach (Transform item in __instance.shopItemsParent.transform)
                {
                    var displaybox = (Component)item;
                    // Add new display field if not already existing
                    if (displaybox.transform.Find("TotalShelvesBCK/ShelvesTotal") == null)
                    {
                        CreateText(item);
                    }

                    // Get counts
                    int productID = item.GetComponent<Data_Product>().productID;
                    var totalCount = 0;
                    var currentProductCount = 0;
                    GetProductsExistences(__instance, productID, ref totalCount, ref currentProductCount);

                    // display counts
                    item.transform.Find("TotalShelvesBCK/ShelvesTotal").GetComponent<TextMeshProUGUI>().text = totalCount.ToString();

                }
            }
            
            // Using similar counter approach as base FixedUpdate method
            fixedCounter++;
            if (fixedCounter >= 15)
            {
                fixedCounter = 0;
            }
        }

        // Method to calculate how many units of the specifed prodcuts can fit on all the reserved shelves
        private static void GetProductsExistences(ManagerBlackboard __instance, int productIDToCompare, ref int totalCount, ref int currentProductCount)
        {

            totalCount = 0;
            currentProductCount = 0;
            if (__instance.dummyArrayExistences.Length > 0)
            {
                // only need to look at dummyArrayExistences[0] as this appears to hold the shelving information
                var containers = __instance.dummyArrayExistences[0];
                foreach (Transform item in containers.transform)
                {
                    var container = item.GetComponent<Data_Container>();
                    var products = container.productInfoArray;
                    int containerCount = products.Length / 2;

                    // loop through each shef in the shelving unit
                    for (int containerNumber = 0; containerNumber < containerCount; containerNumber++)
                    {
                        // check if this shelf contains the product we're interested in
                        var productID = products[containerNumber * 2];
                        var productCount = products[containerNumber * 2 + 1];
                        if (productID != productIDToCompare)
                        {
                            continue;
                        }
                        // determine how many units can fit per shelf
                        GameObject gameObject = container.productlistComponent.productPrefabs[productIDToCompare];
                        Vector3 size = gameObject.GetComponent<BoxCollider>().size;

                        bool isStackable = gameObject.GetComponent<Data_Product>().isStackable;
                        int value = Mathf.FloorToInt(container.shelfLength / (size.x * 1.1f));
                        value = Mathf.Clamp(value, 1, 100);
                        int value2 = Mathf.FloorToInt(container.shelfWidth / (size.z * 1.1f));
                        value2 = Mathf.Clamp(value2, 1, 100);
                        int num = value * value2;
                        if (isStackable)
                        {
                            int value3 = Mathf.FloorToInt(container.shelfHeight / (size.y * 1.1f));
                            value3 = Mathf.Clamp(value3, 1, 100);
                            num = value * value2 * value3;
                        }
                        totalCount += num;
                        currentProductCount += productCount;
                    }
                }

                //Data_Product product = ProductListing.Instance.productPrefabs[productIDToCompare].GetComponent<Data_Product>();
                //Plugin.Logger.LogInfo(string.Format("Shelf total items for {0}, {1}", product.productBrand, totalCount));

            }

        }

        // Create text field to display the Total available count
        private static GameObject CreateText(Component parent)
        {
            var p = parent.gameObject;
            var r = parent.GetComponent<RectTransform>();

            // Create the button GameObject
            GameObject shadowObject = new GameObject("TotalShelvesBCK");
            RectTransform shadowRectTransform = shadowObject.AddComponent<RectTransform>();
            CanvasRenderer shadowCanvasComponent = shadowObject.AddComponent<CanvasRenderer>();
            Image shadowimageComponent = shadowObject.AddComponent<Image>();
            Shadow shadowComponent = shadowObject.AddComponent<Shadow>();

            // Set the button's parent to Buttons_Bar

            shadowObject.transform.SetParent(parent.transform, false);

            // Set up RectTransform properties
            shadowRectTransform.sizeDelta = new Vector2(25, 25); // Adjust width here
            shadowRectTransform.anchoredPosition = new Vector2(92.5f, 27.5f);
            shadowRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            shadowRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            shadowRectTransform.pivot = new Vector2(0.5f, 0.5f);

            shadowComponent.effectColor = new Color(0, 0, 0, 0.5f);
            shadowComponent.effectDistance = new Vector2(2f, -2f);

            shadowimageComponent.color = new Color(0.950f, 0.450f, 1f, 1f);
            shadowimageComponent.type = Image.Type.Sliced;

            // couldn't figure out correct way to create new sprite, so just copying from an exising counter display
            var existingshadow = parent.transform.Find("InShelvesBCK").GetComponent<RectTransform>();
            var existingshadowimagesprite = existingshadow.GetComponent<Image>().sprite;
            shadowimageComponent.sprite = existingshadowimagesprite;

            GameObject textObject = new GameObject("ShelvesTotal");
            textObject.transform.SetParent(shadowObject.transform, false);
            RectTransform textRectTransform = textObject.AddComponent<RectTransform>();
            TextMeshProUGUI textMeshProUGUIComponent = textObject.AddComponent<TextMeshProUGUI>();

            textRectTransform.sizeDelta = new Vector2(20, 20); // Adjust width here
            textRectTransform.anchoredPosition = new Vector2(0, 0);
            textRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            textRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            textRectTransform.pivot = new Vector2(0.5f, 0.5f);

            textMeshProUGUIComponent.alignment = TextAlignmentOptions.Center;
            textMeshProUGUIComponent.color = new Color(0.283f, 0.283f, 0.283f, 1.000f);
            textMeshProUGUIComponent.fontStyle = TMPro.FontStyles.Normal;
            textMeshProUGUIComponent.fontSize = 16;
            textMeshProUGUIComponent.enableAutoSizing = true;
            textMeshProUGUIComponent.fontSizeMin = 4;
            textMeshProUGUIComponent.fontSizeMax = 16;

            return textObject;
        }


        private static int fixedCounter2 = 0;
        private static void LogFirstField(ManagerBlackboard __instance)
        {
            if (fixedCounter2 == 0)
            {
                foreach (Transform item in __instance.shopItemsParent.transform)
                {
                    var shadow = item.transform.Find("InShelvesBCK").GetComponent<RectTransform>();
                    var image = shadow.GetComponent<Image>();

                    Plugin.Logger.LogInfo(string.Format("color {0}", image.color.ToString()));
                    break;
                }
            }
            fixedCounter2++;
            if (fixedCounter2 >= 1500)
            {
                fixedCounter2 = 0;
            }
        }
        //var shadow = ((Component)((Component)item).transform.Find("InShelvesBCK")).GetComponent<RectTransform>();
        //var shadow2 = ((Component)((Component)item).transform.Find("TotalShelvesBCK")).GetComponent<RectTransform>();


        //for (var i = 0; i < item.childCount; i++)
        //{
        //    var child = item.GetChild(i);
        //    var childNames = string.Empty;

        //    foreach (var j in child.GetComponents<Component>()) { childNames += j.name + ":" + j.GetType().ToString() + "-"; }
        //    Plugin.Logger.LogInfo(string.Format("children {0} _ {1}", child.name, childNames));
        //}

        //for (var i = 0; i < shadow.childCount; i++)
        //{
        //    var child = shadow.GetChild(i);
        //    var childNames = string.Empty;

        //    foreach (var j in child.GetComponents<Component>()) { childNames += j.name + ":" + j.GetType().ToString() + "-"; }
        //    Plugin.Logger.LogInfo(string.Format("shadow children {0} _ {1}", child.name, childNames));
        //}
        //Plugin.Logger.LogInfo("");
        //Plugin.Logger.LogInfo("");

        //foreach (var i in shadow.GetComponents<Component>())
        //{
        //    Plugin.Logger.LogInfo("");

        //    Plugin.Logger.LogInfo(string.Format("Shadow {0}:{1}:{2}:{3} Properties", i.name, i.GetType().Name, i.transform.childCount, i.transform.GetComponents<Component>().Length));
        //    var componentNames = string.Empty;
        //    foreach (var j in i.transform.GetComponents<Component>()) { componentNames += j.name + ":" + j.GetType().ToString() + "-"; }
        //    Plugin.Logger.LogInfo(string.Format("components {0} ", componentNames));
        //    foreach (var p in i.GetType().GetProperties())
        //    {
        //        Plugin.Logger.LogInfo(string.Format("{0}:{1}", p.Name, p.GetValue(i, null)));

        //    }
        //}

        //Plugin.Logger.LogInfo("");
        //Plugin.Logger.LogInfo("");

        //foreach (var i in shadow2.GetComponents<Component>())
        //{
        //    Plugin.Logger.LogInfo("");

        //    Plugin.Logger.LogInfo(string.Format("Shadow {0}:{1}:{2}:{3} Properties", i.name, i.GetType().Name, i.transform.childCount, i.transform.GetComponents<Component>().Length));
        //    var componentNames = string.Empty;
        //    foreach (var j in i.transform.GetComponents<Component>()) { componentNames += j.name + ":" + j.GetType().ToString() + "-"; }
        //    Plugin.Logger.LogInfo(string.Format("components {0} ", componentNames));
        //    foreach (var p in i.GetType().GetProperties())
        //    {
        //        Plugin.Logger.LogInfo(string.Format("{0}:{1}", p.Name, p.GetValue(i, null)));
        //    }
        //}
        //var shadowimagesprite = shadow.GetComponent<Image>().sprite;
        //if (shadowimagesprite != null)
        //{
        //    foreach (var s in shadowimagesprite.GetType().GetProperties())
        //    {
        //        Plugin.Logger.LogInfo(string.Format("Sprite {0}:{1}", s.Name, s.GetValue(shadowimagesprite, null)));
        //    }
        //}

        //Plugin.Logger.LogInfo("");
        //Plugin.Logger.LogInfo("");
        ///*
        //for (var i = 0; i < text.childCount; i++)
        //{
        //    var child = text.GetChild(i);
        //    var childNames = string.Empty;

        //    foreach (var j in child.GetComponents<Component>()) { childNames += j.name + ":" + j.GetType().ToString() + "-"; }
        //    Plugin.Logger.LogInfo(string.Format("text children {0} _ {1}", child.name, childNames));
        //}
        //*/

        ///*
        // *
        //Plugin.Logger.LogInfo("Shadow Fields");
        //foreach (var p in shadowfield.GetType().GetProperties())
        //{
        //    Plugin.Logger.LogInfo(string.Format("{0}:{1}", p.Name, p.GetValue(shadowfield, null)));

        //}
        //Plugin.Logger.LogInfo("Text Fields");
        //foreach (var p in textfield.GetType().GetProperties())
        //{
        //    Plugin.Logger.LogInfo(string.Format("{0}:{1}", p.Name, p.GetValue(textfield, null)));
        //}


        //var shadow2field = ((Component)((Component)item).transform.Find("TotalShelvesBCK")).GetComponent<Shadow>();
        //var text2field = ((Component)((Component)item).transform.Find("TotalShelvesBCK/ShelvesTotal")).GetComponent<TextMeshProUGUI>();

        //Plugin.Logger.LogInfo("Shadow2 Fields");
        //foreach (var p in shadow2field.GetType().GetProperties())
        //{
        //    Plugin.Logger.LogInfo(string.Format("{0}:{1}", p.Name, p.GetValue(shadow2field, null)));

        //}
        //Plugin.Logger.LogInfo("Text2 Fields");
        //foreach (var p in text2field.GetType().GetProperties())
        //{
        //    Plugin.Logger.LogInfo(string.Format("{0}:{1}", p.Name, p.GetValue(text2field, null)));
        //}
        //*/
        ///*
        //                    var shadow2 = ((Component)((Component)item).transform.Find("TotalShelvesBCK")).GetComponent<RectTransform>();
        //                    var shadow2field = ((Component)((Component)item).transform.Find("TotalShelvesBCK")).GetComponent<Shadow>();
        //                    Plugin.Logger.LogInfo(string.Format("shadow2 anchor {0}", shadow2.anchoredPosition.ToString()));
        //                    Plugin.Logger.LogInfo(string.Format("shadow2 anchor min {0}", shadow2.anchorMin.ToString()));
        //                    Plugin.Logger.LogInfo(string.Format("shadow2 anchor max {0}", shadow2.anchorMax.ToString()));
        //                    Plugin.Logger.LogInfo(string.Format("shadow2 pivot {0}", shadow2.pivot.ToString()));
        //                    Plugin.Logger.LogInfo(string.Format("shadow2 sizedelta {0}", shadow2.sizeDelta.ToString()));
        //                    Plugin.Logger.LogInfo(string.Format("shadow2 effectColor {0}", shadow2field.effectColor.ToString()));
        //                    var text2 = ((Component)((Component)item).transform.Find("TotalShelvesBCK/ShelvesTotal")).GetComponent<RectTransform>();
        //                    var text2field = ((Component)((Component)item).transform.Find("TotalShelvesBCK/ShelvesTotal")).GetComponent<TextMeshProUGUI>();
        //                    Plugin.Logger.LogInfo(string.Format("text2 anchor {0}", text2.anchoredPosition.ToString()));
        //                    Plugin.Logger.LogInfo(string.Format("text2 anchor min {0}", text2.anchorMin.ToString()));
        //                    Plugin.Logger.LogInfo(string.Format("text2 anchor max {0}", text2.anchorMax.ToString()));
        //                    Plugin.Logger.LogInfo(string.Format("text2 pivot {0}", text2.pivot.ToString()));
        //                    Plugin.Logger.LogInfo(string.Format("text2 sizedelta {0}", text2.sizeDelta.ToString()));
        //                    Plugin.Logger.LogInfo(string.Format("text2 font {0}", text2field.font.ToString()));
        //                    Plugin.Logger.LogInfo(string.Format("text2 color {0}", text2field.color.ToString()));
        //                    Plugin.Logger.LogInfo(string.Format("text2 fontstyle {0}", text2field.fontStyle.ToString()));
        //                    Plugin.Logger.LogInfo(string.Format("text2 alignment {0}", text2field.alignment.ToString()));
        //*/
        ///*
        //var shadow2 = ((Component)((Component)item).transform.Find("InStorageBCK")).GetComponent<RectTransform>();
        //Plugin.Logger.LogInfo(string.Format("shadow2 anchor {0}", shadow2.anchoredPosition.ToString()));
        //Plugin.Logger.LogInfo(string.Format("shadow2 anchor min {0}", shadow2.anchorMin.ToString()));
        //Plugin.Logger.LogInfo(string.Format("shadow2 anchor max {0}", shadow2.anchorMax.ToString()));
        //Plugin.Logger.LogInfo(string.Format("shadow2 pivot {0}", shadow2.pivot.ToString()));
        //Plugin.Logger.LogInfo(string.Format("shadow2 sizedelta {0}", shadow2.sizeDelta.ToString()));
        //var shadow3 = ((Component)((Component)item).transform.Find("InBoxesBCK")).GetComponent<RectTransform>();
        //Plugin.Logger.LogInfo(string.Format("shadow3 anchor {0}", shadow3.anchoredPosition.ToString()));
        //Plugin.Logger.LogInfo(string.Format("shadow3 anchor min {0}", shadow3.anchorMin.ToString()));
        //Plugin.Logger.LogInfo(string.Format("shadow3 anchor max {0}", shadow3.anchorMax.ToString()));
        //Plugin.Logger.LogInfo(string.Format("shadow3 pivot {0}", shadow3.pivot.ToString()));
        //Plugin.Logger.LogInfo(string.Format("shadow3 sizedelta {0}", shadow3.sizeDelta.ToString()));
        //*/
        //break;
        //                }




    }
}
