using NeoFPS.SinglePlayer;
using System.Collections;
using TunnelEffect;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    public class ExtractionFXController : MonoBehaviour
    {
        [SerializeField, Tooltip("The duration the extraction effect should run for.")]
        internal float duration = 8;

        internal enum ExtractionType
        {
            SpawnerDestroyed, // Player destroyed spawner and was extracted
            PlayerEscaped, // Player survived long enough to be extracted
            PortalUsed, // Player used a portal to escape
            Death
        }
        internal ExtractionType extractionType = ExtractionType.PlayerEscaped;
        internal bool isRunning = false;
        private TunnelFX2 extractionFx;

        private void Start()
        {
            extractionFx = GetComponent<TunnelFX2>();
            StartCoroutine(FxUpdateCo());
        }

        private IEnumerator FxUpdateCo()
        {
            switch (extractionType)
            {
                case ExtractionType.SpawnerDestroyed:
                    extractionFx.tintColor = new Color(22, 242, 0);
                    break;
                case ExtractionType.PlayerEscaped:
                    extractionFx.tintColor = new Color(22, 242, 0);
                    break;
                case ExtractionType.PortalUsed:
                    extractionFx.tintColor = new Color(22, 242, 0);
                    break;
                case ExtractionType.Death:
                    extractionFx.tintColor = Color.red;
                    break;
            }

            extractionFx.tintColor = Color.green;

            isRunning = true;

            extractionFx.gameObject.SetActive(true);
            extractionFx.globalAlpha = 0;

            float stepLength = duration / 3;
            extractionFx.transform.SetParent(FpsSoloCharacter.localPlayerCharacter.transform);
            extractionFx.transform.rotation = FpsSoloCharacter.localPlayerCharacter.transform.rotation;
            Vector3 pos = FpsSoloCharacter.localPlayerCharacter.transform.position;

            while (extractionFx.globalAlpha < 1)
            {
                extractionFx.globalAlpha += Time.deltaTime / (stepLength * 2);
                pos.y += Time.deltaTime * 10f;
                extractionFx.transform.position = pos;
                extractionFx.transform.rotation = Quaternion.Euler(Mathf.Lerp(0, -45, extractionFx.globalAlpha), 0, 0);
                yield return null;
            }

            yield return new WaitForSeconds(stepLength);

            isRunning = false;

            extractionFx.gameObject.SetActive(false);
            extractionFx.transform.rotation = Quaternion.identity;
            extractionFx.transform.SetParent(null);

            yield return null;
        }

        private void OnValidate()
        {
            if (gameObject.activeSelf)
            {
                extractionFx.gameObject.SetActive(false);
            }
        }
    }
}
