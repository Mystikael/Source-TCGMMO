using System.Collections;
using System.Text;
using SourceTCG.Core;
using SourceTCG.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SourceTCG.UI
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] Text inventoryText;
        [SerializeField] Button backButton;

        SourceApiClient Api => SourceSession.Instance?.Api;

        void Awake()
        {
            backButton?.onClick.AddListener(() => SceneManager.LoadScene("WorldMap"));
        }

        void Start() => StartCoroutine(LoadInventory());

        IEnumerator LoadInventory()
        {
            if (Api == null)
            {
                if (inventoryText != null) inventoryText.text = "No session.";
                yield break;
            }
            yield return Api.RefreshWallet();
            InventoryResponse inv = null;
            yield return Api.GetInventory(r => inv = r);
            var sb = new StringBuilder();
            sb.AppendLine("INVENTORY");
            sb.AppendLine($"Source Points: {Api.Wallet?.sourcePoints ?? 0}");
            sb.AppendLine();
            if (inv?.items != null && inv.items.Length > 0)
            {
                foreach (var item in inv.items)
                    sb.AppendLine($"{item.item_type} {item.subtype_id} x{item.quantity} (tier {item.tier})");
            }
            else sb.AppendLine("(empty)");
            if (inventoryText != null) inventoryText.text = sb.ToString();
        }
    }
}