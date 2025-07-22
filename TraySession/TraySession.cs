namespace TraySession
{

    /// <summary>
    /// The logic that controls interaction with the game session. <para/>
    /// 
    /// NOTE: A good way to handle atomicity within this class is to consider using immutable types (PlayerState, and Tray).
    /// This way, modification is always made on a new copy, and only swapped for the old copy if all transactions are complete,
    /// else discard the updates.
    /// </summary>
    public record TraySessionController
    {
        #region necessary services
        private readonly IDeferred<ITrayManager> _trayManager;
        private readonly IDeferred<IChanceManager> _chanceManager;
        private readonly IDeferred<IQueuedPickManager> _queuedPickManager;
        private readonly IDeferred<ILivePlayerStateManager> _playerStateManager;
        #endregion

        #region Internal operation scheduling
        private readonly BlockingConcurrentQueue<AsyncOp<Optional<Shot>>> _registrationQueue = new();
        private readonly BlockingConcurrentQueue<AsyncOp<ImmutableArray<Pick>>> _pickAssignmentQueue = new();
        #endregion

        public Suid SessionId { get; }

        public LivePlayerState PlayerState { get; }

        public Tray Tray { get; }

        public GridPreset Preset { get; }


        public TraySessionController(
            Tray tray,
            LivePlayerState playerState,
            GridPreset preset,
            IDeferred<ITrayManager> trayManager,
            IDeferred<IChanceManager> chanceManager,
            IDeferred<IQueuedPickManager> queuedPickManager,
            IDeferred<ILivePlayerStateManager> playerStateManager)
        {
            ArgumentNullException.ThrowIfNull(playerState);
            ArgumentNullException.ThrowIfNull(tray);
            ArgumentNullException.ThrowIfNull(preset);
            ArgumentNullException.ThrowIfNull(trayManager);
            ArgumentNullException.ThrowIfNull(chanceManager);
            ArgumentNullException.ThrowIfNull(queuedPickManager);
            ArgumentNullException.ThrowIfNull(playerStateManager);

            if (tray.SessionId != playerState.SessionId)
                throw new InvalidOperationException(
                    $"SessionId mismatch [Tray: {tray.SessionId}, PlayerState: {playerState.SessionId}]");

            // assign services
            _trayManager = trayManager;
            _chanceManager = chanceManager;
            _queuedPickManager = queuedPickManager;
            _playerStateManager = playerStateManager;

            // assign states
            SessionId = tray.SessionId;
            PlayerState = playerState;
            Tray = tray;
            Preset = preset;

            // do other validation

            // initialize queue handlers

            // setup Session timers
        }

        #region Session info/data
        #endregion

        #region Queue Handlers
        #endregion

        #region Session Handlers
        #endregion

        #region Player Interactions
        public bool IsPlayerRegistered(Suid playerId) => PlayerState.HasPlayer(playerId);

        /// <summary>
        /// A player attempts to register an amount of chances for this tray session. This is the entry into
        /// the tray session for every player - i.e, the only way a player gets added to <see cref="PlayerState"/>.
        /// </summary>
        /// <param name="playerId">The player id</param>
        /// <param name="chanceCount">The chance count</param>
        /// <returns>The shot, or an empty optional instance</returns>
        public Task<Optional<Shot>> RegisterLiveChances(Suid playerId, ushort chanceCount)
        {
            var asyncOp = new AsyncOp<Optional<Shot>>
            {
                TaskSource = new TaskCompletionSource<Optional<Shot>>(),
                Operation = async tcs =>
                {
                    try
                    {
                        // player already has registered shots?
                        if (PlayerState.HasPlayer(playerId))
                            tcs.SetResult(Optional<Shot>.Empty());

                        // Can we add at least 1 chance?
                        else if (Tray.FreeTileCount == ushort.MinValue)
                            tcs.SetResult(Optional<Shot>.Empty());

                        else
                        {
                            var chanceDelta = Math.Min(chanceCount, Tray.FreeTileCount);

                            // retrieve the Shot from the ChanceManager
                            var cm = _chanceManager.Resolve();
                            var optionalShot = await cm.AccumulateExpendableChances(new()
                            {
                                PlayerId = playerId,
                                PresetId = Preset.Id,
                                ChanceCount = chanceDelta
                            });

                            // expend
                            await optionalShot.ConsumeOptionalAsync(async shot => await shot.ChanceMap
                                .ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value.CurrentValue)
                                .ApplyTo(map => new IChanceManager.ExpendChanceRequest { ChanceMap = map })
                                .ApplyTo(cm.ExpendChances));

                            // register
                            optionalShot.ConsumeOptional(shot =>
                            {
                                // TODO: This is a severe case. The expended shots need to be retrieved
                                // so the system is back in a stable state.
                                if (!PlayerState.TryRegisterShot(shot))
                                    tcs.SetException(new InvalidOperationException(
                                        $"Invalid state [playerId: {playerId}, msg: Could not register shot]"));
                            });

                            var psm = _playerStateManager.Resolve();
                            await psm.UpdatePlayerState(PlayerState);
                            tcs.SetResult(optionalShot);
                        }
                    }
                    catch (Exception error)
                    {
                        tcs.SetException(error);
                    }
                }
            };

            _registrationQueue.Enqueue(asyncOp);
            return asyncOp.TaskSource.Task;
        }

        /// <summary>
        /// Expand the number of chances by a given value. This method will fail if the player isn't already present
        /// in the <see cref="PlayerState"/> instance.
        /// </summary>
        /// <param name="playerId">The player id</param>
        /// <param name="additionalChances">The chance delta to add</param>
        /// <returns>The expanded shot, or an empty optional instance if the operation failed</returns>
        public Task<Optional<Shot>> AppendChances(Suid playerId, ushort additionalChances)
        {
            var asyncOp = new AsyncOp<Optional<Shot>>
            {
                TaskSource = new TaskCompletionSource<Optional<Shot>>(),
                Operation = async tcs =>
                {
                    try
                    {
                        // player already has not registered shots?
                        if (!PlayerState.HasPlayer(playerId))
                            tcs.SetResult(Optional<Shot>.Empty());

                        // Can we add at least 1 chance?
                        else if (Tray.FreeTileCount == ushort.MinValue)
                            tcs.SetResult(Optional<Shot>.Empty());

                        else
                        {
                            var chanceDelta = Math.Min(additionalChances, Tray.FreeTileCount);

                            // retrieve the Shot from the ChanceManager
                            var cm = _chanceManager.Resolve();
                            var optionalShot = await cm.AccumulateExpendableChances(new()
                            {
                                PlayerId = playerId,
                                PresetId = Preset.Id,
                                ChanceCount = chanceDelta
                            });

                            // expend
                            await optionalShot.ConsumeOptionalAsync(async shot => await shot.ChanceMap
                                .ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value.CurrentValue)
                                .ApplyTo(map => new IChanceManager.ExpendChanceRequest { ChanceMap = map })
                                .ApplyTo(cm.ExpendChances));

                            // register
                            optionalShot.ConsumeOptional(shot =>
                            {
                                // TODO: This is a severe case. The expended shots need to be retrieved
                                // so the system is back in a stable state.
                                if (!PlayerState.TryMergeShot(shot))
                                    tcs.SetException(new InvalidOperationException(
                                        $"Invalid state [playerId: {playerId}, msg: Could not register shot]"));
                            });

                            var psm = _playerStateManager.Resolve();
                            await psm.UpdatePlayerState(PlayerState);
                            tcs.SetResult(optionalShot);
                        }
                    }
                    catch (Exception error)
                    {
                        tcs.SetException(error);
                    }
                }
            };

            _registrationQueue.Enqueue(asyncOp);
            return asyncOp.TaskSource.Task;
        }

        /// <summary>
        /// A player attempts to pick a tile. This operation uses a <see cref="BlockingConcurrentQueue{TValue}"/>
        /// to funnel pick-requests and process them sequentially in a first-come-first-serve manner.
        /// </summary>
        /// <param name="playerId">The player id</param>
        /// <param name="tile">The tile to pick</param>
        /// <returns>The pick instance if successful, or an empty instance otherwise</returns>
        public Task<Optional<Pick>> AssignLivePick(Suid playerId, Tile tile)
        {
            var asyncOp = new AsyncOp<ImmutableArray<Pick>>
            {
                TaskSource = new TaskCompletionSource<ImmutableArray<Pick>>(),
                Operation = async tcs =>
                {
                    try
                    {
                        // Tile registered
                        if (Tray.IsTileAssigned(tile))
                            tcs.SetResult([]);

                        // Get player's registered shots
                        else if (!PlayerState.TryGetShot(playerId, out var shot))
                            tcs.SetResult([]);

                        // Player has enouch un-expended chances?
                        else if (!shot.Resolve().TryExpendAndPick(PickSource.Live, tile, out var pick))
                            tcs.SetResult([]);

                        // assign to tray
                        else if (!Tray.TryAssign(pick.Resolve()))
                            tcs.SetResult([]);

                        else
                        {
                            // persist PlayerState
                            await _playerStateManager
                                .Resolve()
                                .UpdatePlayerState(PlayerState);

                            // persist tray
                            await _trayManager
                                .Resolve()
                                .UpdateTray(Tray);

                            tcs.SetResult([pick.Resolve()]);
                        }
                    }
                    catch (Exception error)
                    {
                        tcs.SetException(error);
                    }
                }
            };

            _pickAssignmentQueue.Enqueue(asyncOp);
            return asyncOp.TaskSource.Task.Then(picks => picks.FirstOrOptional());
        }

        /// <summary>
        /// Evaluates a quota of <see cref="QueuedPick"/> instances to assign to the <see cref="Tray"/>.
        /// As with <see cref="AssignLivePick(Suid, Tile)"/>, adds the given queued picks to the queue to be
        /// processed. <para/>
        /// This method is triggered:
        /// <list type="number">
        /// <item>First, as soon as the tray is created</item>
        /// <item>Subsequently, after a given interval has passed and the Tray is not fully assigned</item>
        /// </list>
        /// </summary>
        /// <returns>The assigned picks</returns>
        public Task<ImmutableArray<Pick>> AssignQueuedPicks()
        {
            var asyncOp = new AsyncOp<ImmutableArray<Pick>>
            {
                TaskSource = new TaskCompletionSource<ImmutableArray<Pick>>(),
                Operation = async tcs =>
                {
                    try
                    {
                        // 1. Retrieve queued pick quota
                        var qpm = _queuedPickManager.Resolve();
                        var queuedPicks = await qpm.GetNextPresetQuota(new()
                        {
                            TraySize = Tray.TraySize,
                            QueuedPickQuota = Preset.QueuedPickQuotaPercentage,
                            ExcludedTileIndexes = [.. Tray.Picks.Select(pick => pick.Tile)],
                            ExcludedPlayers = [
                                ..Tray.Picks.Select(pick => pick.PlayerId), // exclude players who already have picks (queued or otherwise)
                            ..PlayerState.PlayerShots.Select(shot => shot.PlayerId)] // exclude registered players
                        });

                        // 2. Assign quota to tray
                        var assignablePicks = queuedPicks
                            .SelectMany(pick => pick.ToPicks())
                            .ToImmutableArray();

                        if (!assignablePicks
                            .Select(Tray.TryAssign)
                            .All(assigned => assigned))
                        {
                            // if this happens, unassign all assigned picks
                            assignablePicks.ForAll(pick => Tray.TryUnassign(pick));

                            tcs.SetException(new InvalidOperationException(
                                $"Invalid state: tile assignment failed"));
                            return;
                        }

                        // 3. Persist assigned queued picks
                        await qpm.UpdatePickStatus(new()
                        {
                            NewStatus = QueuedPickStatus.Assigned,
                            QueuedPicks = queuedPicks
                        });

                        // 4. Persist tray
                        var tm = _trayManager.Resolve();
                        await tm.UpdateTray(Tray);

                        tcs.SetResult(assignablePicks);
                    }
                    catch (Exception error)
                    {
                        tcs.SetException(error);
                    }
                }
            };

            _pickAssignmentQueue.Enqueue(asyncOp);
            return asyncOp.TaskSource.Task;
        }
        #endregion


        #region Nested types
        internal record AsyncOp<TResult>
        {
            required public Func<TaskCompletionSource<TResult>, Task> Operation { get; init; }

            required public TaskCompletionSource<TResult> TaskSource { get; init; }
        }
        #endregion
    }
}
