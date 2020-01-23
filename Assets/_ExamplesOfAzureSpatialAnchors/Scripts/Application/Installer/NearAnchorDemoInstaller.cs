using GATARI.ExamplesOfAzureSpatialAnchors.Domain.UseCase.Impl.Navigation;
using GATARI.ExamplesOfAzureSpatialAnchors.Presentation.Presenter.Impl.Common;
using GATARI.ExamplesOfAzureSpatialAnchors.Presentation.Presenter.Impl.Controller;
using GATARI.ExamplesOfAzureSpatialAnchors.Presentation.View.Common;
using Microsoft.Azure.SpatialAnchors;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Zenject;

namespace GATARI.ExamplesOfAzureSpatialAnchors.Application.Installer
{
    public class NearAnchorDemoInstaller : MonoInstaller<NearAnchorDemoInstaller>
    {
        [SerializeField] private GameObject spatialAnchorPrefab = default;
        public override void InstallBindings()
        {
            // Factory
            var arSessionOrigin = FindObjectOfType<ARSessionOrigin>();

            Container.BindFactory<CloudSpatialAnchor, SpatialAnchorView, SpatialAnchorViewFactory>()
                .FromComponentInNewPrefab(spatialAnchorPrefab)
                .UnderTransform(arSessionOrigin.transform)
                .AsCached();

            // UseCase
            Container.BindInterfacesTo<NearAnchorDemoUseCase>()
                .AsCached();

            // Presenter
            Container.BindInterfacesTo<InstructionUIPresenter>()
                .AsCached();
            Container.BindInterfacesTo<AnchorObjectPresenter>()
                .AsCached();
            Container.BindInterfacesTo<LeanTouchController>()
                .AsCached();
        }
    }
}