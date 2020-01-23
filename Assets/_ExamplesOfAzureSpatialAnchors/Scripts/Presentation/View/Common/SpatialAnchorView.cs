using GATARI.ExamplesOfAzureSpatialAnchors.Presentation.Presenter.Interface.Common;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using UnityEngine;
using Zenject;

namespace GATARI.ExamplesOfAzureSpatialAnchors.Presentation.View.Common
{
    public class SpatialAnchorView : MonoBehaviour, ISpatialAnchorView
    {
        public GameObject GameObject => gameObject;
        public CloudNativeAnchor CloudNativeAnchor => _cloudNativeAnchor;

        private CloudNativeAnchor _cloudNativeAnchor;

        [Inject]
        private void Construct(CloudSpatialAnchor cloudSpatialAnchor)
        {
            _cloudNativeAnchor = gameObject.AddComponent<CloudNativeAnchor>();

            // restore existing anchor if cloudSpatialAnchor is not null
            // otherwise it is new one
            if (cloudSpatialAnchor == null) return;

            _cloudNativeAnchor.CloudToNative(cloudSpatialAnchor);
        }
    }

    public class SpatialAnchorViewFactory : PlaceholderFactory<CloudSpatialAnchor, SpatialAnchorView>
    {
    }
}