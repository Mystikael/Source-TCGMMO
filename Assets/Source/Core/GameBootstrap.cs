using SourceTCG.UI;
using UnityEngine;

namespace SourceTCG.Core
{
    /// <summary>Legacy bootstrap hook — login flow is handled by LoginController.</summary>
    public class GameBootstrap : MonoBehaviour
    {
        void Awake()
        {
            if (GetComponent<LoginController>() == null)
                gameObject.AddComponent<LoginController>();
        }
    }
}