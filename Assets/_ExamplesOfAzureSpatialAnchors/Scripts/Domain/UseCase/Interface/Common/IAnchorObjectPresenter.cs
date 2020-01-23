using CAFU.Core;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using UnityEngine;

namespace GATARI.ExamplesOfAzureSpatialAnchors.Domain.UseCase.Interface.Common
{
    public interface IAnchorObjectPresenter : IPresenter
    {
        bool IsPlacingAnchorExists { get; }
        void CreateNewAnchor(Vector3 position);
        void MovePlacingAnchor(Vector3 position);
        CloudSpatialAnchor ConfirmNewAnchorAndGetCloudSpatialAnchor();
        void RestoreAnchorObject(CloudSpatialAnchor cloudSpatialAnchor, Color color, Vector3 scale);
        void CleanAllAnchors();
    }
}