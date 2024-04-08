using NeoFPS;
using NeoFPS.Constants;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    public class BuildingSurface : BaseSurface
    {
        [SerializeField, Tooltip("The surface material for this building.")]
        private FpsSurfaceMaterial m_Surface = FpsSurfaceMaterial.Default;

        internal FpsSurfaceMaterial Surface {
            get { return m_Surface; }
            set { m_Surface = value; }
        }

        public override FpsSurfaceMaterial GetSurface()
        {
            return m_Surface;
        }
        public override FpsSurfaceMaterial GetSurface(RaycastHit hit)
        {
            return m_Surface;
        }
        public override FpsSurfaceMaterial GetSurface(ControllerColliderHit hit)
        {
            return m_Surface;
        }
    }
}