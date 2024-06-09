using NeoFPS;
using NeoFPS.Constants;
using ProceduralToolkit.Buildings;
using UnityEngine;

namespace RogueWave
{
    public class BuildingSurface : BaseSurface
    {
        private FpsSurfaceMaterial m_Surface = FpsSurfaceMaterial.CrystalAggregate;

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