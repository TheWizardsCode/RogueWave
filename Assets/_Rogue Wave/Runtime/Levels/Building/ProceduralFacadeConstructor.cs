using System.Collections.Generic;
using NeoFPS.Constants;
using ProceduralToolkit;
using ProceduralToolkit.Buildings;
using UnityEngine;

namespace RogueWave.Procedural
{
    [CreateAssetMenu(menuName = "Rogue Wave/Buildings/Procedural Facade Constructor", order = 3)]
    public class ProceduralFacadeConstructor : FacadeConstructor
    {
        [SerializeField, Tooltip("The surface material for this building.")]
        internal FpsSurfaceMaterial surface = FpsSurfaceMaterial.CrystalAggregate;
        [SerializeField, Tooltip("The colour range for the walls for this building. Upon generation a random selection will be made from this gradient.")]
        public Gradient wallGradient = default;

        [SerializeField]
        private RendererProperties rendererProperties = null;
        [SerializeField]
        private Material glassMaterial = null;
        [SerializeField]
        private Material roofMaterial = null;
        [SerializeField]
        private Material wallMaterial = null;

        private Palette m_palette;

        internal Palette palette
        {
            get
            {
                if (m_palette == null)
                {
                    m_palette = new Palette();
                }
                float gradientValue = Random.value;
                m_palette.wallColor = wallGradient.Evaluate(gradientValue);
                m_palette.glassColor = wallGradient.Evaluate(gradientValue * 0.5f);
                m_palette.frameColor = wallGradient.Evaluate(gradientValue * 0.56f);

                return m_palette;
            }
        }

        public override void Construct(List<Vector2> foundationPolygon, List<ILayout> layouts, Transform parentTransform)
        {
            var facadesDraft = new CompoundMeshDraft();

            var rendererGo = new GameObject("Facades");
            rendererGo.transform.SetParent(parentTransform, false);

            for (var i = 0; i < layouts.Count; i++)
            {
                var layout = layouts[i];

                Vector2 a = foundationPolygon.GetLooped(i + 1);
                Vector2 b = foundationPolygon[i];
                Vector3 normal = (b - a).Perp().ToVector3XZ();

                var facade = new CompoundMeshDraft();
                ConstructLayout(facade, Vector2.zero, layout);
                facade.Rotate(Quaternion.LookRotation(normal));
                facade.Move(a.ToVector3XZ());
                facadesDraft.Add(facade);
            }

            facadesDraft.MergeDraftsWithTheSameName();
            facadesDraft.SortDraftsByName();

            var meshFilter = rendererGo.gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = facadesDraft.ToMeshWithSubMeshes();

            var meshRenderer = rendererGo.gameObject.AddComponent<MeshRenderer>();
            meshRenderer.ApplyProperties(rendererProperties);

            var materials = new List<Material>();
            foreach (var draft in facadesDraft)
            {
                if (draft.name == "Glass")
                {
                    materials.Add(glassMaterial);
                }
                else if (draft.name == "Roof")
                {
                    materials.Add(roofMaterial);
                }
                else if (draft.name == "Wall")
                {
                    materials.Add(wallMaterial);
                }
            }
            meshRenderer.materials = materials.ToArray();
        }

        public static void ConstructLayout(CompoundMeshDraft draft, Vector2 parentLayoutOrigin, ILayout layout)
        {
            foreach (var element in layout)
            {
                ConstructElement(draft, parentLayoutOrigin + layout.origin, element);
            }
        }

        public static void ConstructElement(CompoundMeshDraft draft, Vector2 parentLayoutOrigin, ILayoutElement element)
        {
            var layout = element as ILayout;
            if (layout != null)
            {
                ConstructLayout(draft, parentLayoutOrigin, layout);
                return;
            }
            var constructible = element as IConstructible<CompoundMeshDraft>;
            if (constructible != null)
            {
                draft.Add(constructible.Construct(parentLayoutOrigin));
            }
        }
    }
}
