using GATARI.ExamplesOfAzureSpatialAnchors.Domain.Structure;
using UniRx.Async;
using UnityEngine.SceneManagement;
using Zenject;

namespace GATARI.ExamplesOfAzureSpatialAnchors.Domain.UseCase.Impl.System
{
    public class AppInitializeUseCase : IInitializable
    {
        [Inject] private readonly ZenjectSceneLoader _zenjectSceneLoader = default;

        public async void Initialize()
        {
            await _zenjectSceneLoader.LoadSceneAsync(nameof(SceneName.NearAnchorDemo), LoadSceneMode.Additive);
        }
    }
}