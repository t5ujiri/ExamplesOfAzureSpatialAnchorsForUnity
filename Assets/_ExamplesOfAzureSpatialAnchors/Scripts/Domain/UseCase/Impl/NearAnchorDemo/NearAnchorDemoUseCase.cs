using System;
using System.Collections.Generic;
using System.Linq;
using GATARI.ExamplesOfAzureSpatialAnchors.Domain.UseCase.Interface.Common;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using UniRx;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace GATARI.ExamplesOfAzureSpatialAnchors.Domain.UseCase.Impl.Navigation
{
    public class NearAnchorDemoUseCase : IInitializable, IDisposable
    {
        [Inject] private readonly SpatialAnchorManager _spatialAnchorManager = default;
        [Inject] private readonly IInstructionUIPresenter _instructionUIPresenter = default;
        [Inject] private readonly ITouchController _touchController = default;
        [Inject] private readonly IAnchorObjectPresenter _anchorObjectPresenter = default;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public void Initialize()
        {
            if (string.IsNullOrEmpty(_spatialAnchorManager.SpatialAnchorsAccountId) | string.IsNullOrEmpty(_spatialAnchorManager.SpatialAnchorsAccountKey))
            {
                throw new ArgumentNullException(nameof(_spatialAnchorManager.SpatialAnchorsAccountId));
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer | Application.platform == RuntimePlatform.Android)
            {
                DemoRoutine().ToObservable()
                    .ObserveOnMainThread()
                    .SubscribeOnMainThread()
                    .Subscribe(
                        _ => { },
                        Application.Quit
                    );
            }
            else
            {
                Debug.LogWarning("Editorでは動作しません！");
            }
        }

        private async UniTask DemoRoutine()
        {
            _instructionUIPresenter.UpdateMessage("NearAnchor Demo");
            await _instructionUIPresenter.OnTriggerProceed().First();

            _instructionUIPresenter.UpdateMessage("1.最初に一つ目のアンカーを生成します");
            await _instructionUIPresenter.OnTriggerProceed().First();
            //　セッション開始
            await _spatialAnchorManager.StartSessionAsync();
            // 最初のアンカーの生成
            var firstAnchor = await CreateAnchorTask();

            _instructionUIPresenter.UpdateMessage("2.続けて周辺に接続されたアンカーを生成します");
            await _instructionUIPresenter.OnTriggerProceed().First();

            _instructionUIPresenter.UpdateMessage("3.二つ目のアンカーを生成します");
            await _instructionUIPresenter.OnTriggerProceed().First();
            // 周辺のアンカーの生成
            var secondAnchor = await CreateAnchorTask();

            _instructionUIPresenter.UpdateMessage("4.三つ目のアンカーを生成します");
            await _instructionUIPresenter.OnTriggerProceed().First();
            var thirdAnchor = await CreateAnchorTask();

            _instructionUIPresenter.UpdateMessage("5.一度セッションを終了します");
            await _instructionUIPresenter.OnTriggerProceed().First();
            await _spatialAnchorManager.ResetSessionAsync();

            _instructionUIPresenter.UpdateMessage("6.アンカーが配置されていることを確認します");
            await _instructionUIPresenter.OnTriggerProceed().First();
            await GraphAnchorsTask(new[] {firstAnchor, secondAnchor, thirdAnchor}, Color.blue, new Vector3(0.08f, 0.2f, 0.08f));

            _instructionUIPresenter.UpdateMessage("7.もう一度セッションを終了します");
            await _instructionUIPresenter.OnTriggerProceed().First();
            await _spatialAnchorManager.ResetSessionAsync();

            _instructionUIPresenter.UpdateMessage("8.一つ目のアンカーを画像情報から検知します");
            await _instructionUIPresenter.OnTriggerProceed().First();
            var locatedFirstAnchor = (await LocateAnchorsTask(new[] {firstAnchor}, Color.green, new Vector3(0.2f, 0.08f, 0.08f))).ToList();
            Assert.IsTrue(locatedFirstAnchor.Count == 1);

            _instructionUIPresenter.UpdateMessage("9.残り二つのアンカーを接続情報から検知します");
            await _instructionUIPresenter.OnTriggerProceed().First();
            await LocateNearAnchorsTask(locatedFirstAnchor[0], 2);

            _instructionUIPresenter.UpdateMessage("10.セッションを終了します");
            await _instructionUIPresenter.OnTriggerProceed().First();
            _anchorObjectPresenter.CleanAllAnchors();
            _spatialAnchorManager.DestroySession();
        }

        private async UniTask<string> CreateAnchorTask()
        {
            _instructionUIPresenter.UpdateMessage("画面をタップしてアンカーを設置してください");
            var placementDisposable = _touchController.GetWorldTouchOnScreen()
                .Subscribe(screenPoint =>
                {
                    var pose = _touchController.GetARRaycastHitPose(screenPoint);
                    if (_anchorObjectPresenter.IsPlacingAnchorExists)
                    {
                        _anchorObjectPresenter.MovePlacingAnchor(pose.position);
                    }
                    else
                    {
                        _anchorObjectPresenter.CreateNewAnchor(pose.position);
                    }
                }).AddTo(_compositeDisposable);
            await _instructionUIPresenter.OnTriggerProceed().Where(_ => _anchorObjectPresenter.IsPlacingAnchorExists).First();
            placementDisposable.Dispose();

            if (!_spatialAnchorManager.IsReadyForCreate)
            {
                await Observable.EveryUpdate()
                    .TakeWhile(_ => !_spatialAnchorManager.IsReadyForCreate | _spatialAnchorManager.SessionStatus.RecommendedForCreateProgress < 1)
                    .ObserveOnMainThread()
                    .Do(_ =>
                    {
                        var progress = _spatialAnchorManager.SessionStatus.RecommendedForCreateProgress * 100;
                        _instructionUIPresenter.UpdateMessage($"環境をスキャンして特徴点データを収集してください {progress:F1}% ");
                    });
            }

            var cloudAnchor = _anchorObjectPresenter.ConfirmNewAnchorAndGetCloudSpatialAnchor();
            cloudAnchor.Expiration = DateTimeOffset.UtcNow + TimeSpan.FromMinutes(10);

            _instructionUIPresenter.UpdateMessage("アンカーを保存中...");
            await _spatialAnchorManager.CreateAnchorAsync(cloudAnchor);

            _instructionUIPresenter.UpdateMessage($"アンカーの保存が完了しました! ID: {cloudAnchor.Identifier}");
            await _instructionUIPresenter.OnTriggerProceed().First();

            return cloudAnchor.Identifier;
        }

        private async UniTask<IEnumerable<CloudSpatialAnchor>> LocateAnchorsTask(string[] identifiers, Color color, Vector3 scale)
        {
            var locateCriteria = new AnchorLocateCriteria()
            {
                Identifiers = identifiers,
                Strategy = LocateStrategy.VisualInformation
            };

            var watcher = _spatialAnchorManager.Session.CreateWatcher(locateCriteria);

            var locatedAnchorCount = 0;
            _instructionUIPresenter.UpdateMessage($"アンカーを画像情報から検知中... {locatedAnchorCount}/{identifiers.Length}");

            var anchorLocatedEventArgs = await Observable.FromEvent<AnchorLocatedDelegate, AnchorLocatedEventArgs>(
                    h => (sender, e) => h.Invoke(e),
                    h => _spatialAnchorManager.AnchorLocated += h,
                    h => _spatialAnchorManager.AnchorLocated -= h)
                .Where(e => e.Status == LocateAnchorStatus.Located)
                .Distinct(f => f.Identifier)
                .ObserveOnMainThread()
                .Do(e =>
                {
                    locatedAnchorCount++;
                    _anchorObjectPresenter.RestoreAnchorObject(e.Anchor, color, scale);
                    _instructionUIPresenter.UpdateMessage($"アンカーを画像情報から検知中... {locatedAnchorCount}/{identifiers.Length}");
                })
                .Take(identifiers.Length)
                .ToArray();

            watcher.Stop();
            _instructionUIPresenter.UpdateMessage($"{identifiers.Length}個の全てのアンカーを検知しました！");
            await _instructionUIPresenter.OnTriggerProceed().First();

            return anchorLocatedEventArgs.Select(e => e.Anchor);
        }

        private async UniTask GraphAnchorsTask(string[] identifiers, Color color, Vector3 scale)
        {
            var nearAnchorCriteria = new NearAnchorCriteria();

            var watcher = _spatialAnchorManager.Session.CreateWatcher(
                new AnchorLocateCriteria()
                {
                    Identifiers = identifiers,
                    NearAnchor = nearAnchorCriteria,
                    Strategy = LocateStrategy.VisualInformation
                });

            var detectedAnchorCount = 0;
            _instructionUIPresenter.UpdateMessage($"アンカーを検知中... {detectedAnchorCount}/{identifiers.Length}");

            var detectedAnchors = await Observable.FromEvent<AnchorLocatedDelegate, AnchorLocatedEventArgs>(
                    h => (sender, e) => h.Invoke(e),
                    h => _spatialAnchorManager.AnchorLocated += h,
                    h => _spatialAnchorManager.AnchorLocated -= h)
                .Where(e => e.Status == LocateAnchorStatus.Located)
                .Distinct(f => f.Identifier)
                .ObserveOnMainThread()
                .Do(e =>
                {
                    detectedAnchorCount++;
                    _instructionUIPresenter.UpdateMessage($"アンカーを検知中... {detectedAnchorCount}/{identifiers.Length}");
                    _anchorObjectPresenter.RestoreAnchorObject(e.Anchor, color, scale);
                })
                .Take(identifiers.Length)
                .ToArray();

            watcher.Stop();
            _instructionUIPresenter.UpdateMessage($"{detectedAnchors.Length}個の全てのアンカーを検知しました！");
            await _instructionUIPresenter.OnTriggerProceed().First();
        }

        private async UniTask LocateNearAnchorsTask(CloudSpatialAnchor sourceAnchor, int count)
        {
            var nearAnchorCriteria = new NearAnchorCriteria()
            {
                SourceAnchor = sourceAnchor
            };

            var watcher = _spatialAnchorManager.Session.CreateWatcher(
                new AnchorLocateCriteria()
                {
                    NearAnchor = nearAnchorCriteria,
                    Strategy = LocateStrategy.Relationship
                });

            var detectedAnchorCount = 0;
            _instructionUIPresenter.UpdateMessage($"アンカーを接続情報から検知中... {detectedAnchorCount}/{count}");

            var detectedAnchors = await Observable.FromEvent<AnchorLocatedDelegate, AnchorLocatedEventArgs>(
                    h => (sender, e) => h.Invoke(e),
                    h => _spatialAnchorManager.AnchorLocated += h,
                    h => _spatialAnchorManager.AnchorLocated -= h)
                .Where(e => e.Status == LocateAnchorStatus.Located)
                .Distinct(f => f.Identifier)
                .ObserveOnMainThread()
                .Do(e =>
                {
                    detectedAnchorCount++;
                    _instructionUIPresenter.UpdateMessage($"アンカーを接続情報から検知中... {detectedAnchorCount}/{count}");
                    _anchorObjectPresenter.RestoreAnchorObject(e.Anchor, Color.blue, new Vector3(0.2f, 0.08f, 0.08f));
                })
                .Take(count)
                .ToArray();

            watcher.Stop();
            _instructionUIPresenter.UpdateMessage($"{detectedAnchors.Length}個の全てのアンカーを検知しました！");
            await _instructionUIPresenter.OnTriggerProceed().First();
        }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
            _anchorObjectPresenter.CleanAllAnchors();

            if (_spatialAnchorManager.Session != null)
            {
                _spatialAnchorManager.DestroySession();
            }
        }
    }
}