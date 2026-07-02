using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace SourceTCG.Tests.EditMode
{
    public class SceneWiringEditModeTests
    {
        [Test]
        public void WorldMapScene_YamlContainsSerializedVisualComponents()
        {
            var path = Path.Combine(Application.dataPath, "Scenes/WorldMap.unity");
            var text = File.ReadAllText(path);
            Assert.That(text, Does.Contain("MapPinVisualizer"));
            Assert.That(text, Does.Contain("KiProgressBar"));
            Assert.That(text, Does.Contain("pinPanel:"));
            Assert.That(text, Does.Contain("m_Name: PinPanel"));
            Assert.That(text, Does.Contain("m_Name: Canvas"));
        }
    }
}