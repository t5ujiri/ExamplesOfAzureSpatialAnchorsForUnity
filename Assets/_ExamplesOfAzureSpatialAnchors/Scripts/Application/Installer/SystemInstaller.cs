using GATARI.ExamplesOfAzureSpatialAnchors.Domain.UseCase.Impl.System;
using Zenject;

namespace GATARI.ExamplesOfAzureSpatialAnchors.Application.Installer
{
    public class SystemInstaller : MonoInstaller<SystemInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<AppInitializeUseCase>().AsCached();
        }
    }
}