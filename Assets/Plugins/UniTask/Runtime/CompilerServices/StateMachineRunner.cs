﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Cysharp.Threading.Tasks.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Cysharp.Threading.Tasks.CompilerServices
{
    internal interface IStateMachineRunner
    {
        Action MoveNext { get; }
        void Return();
    }

    internal interface IStateMachineRunnerPromise : IUniTaskSource
    {
        Action MoveNext { get; }
        UniTask Task { get; }
        void SetResult();
        void SetException(Exception exception);
    }

    internal interface IStateMachineRunnerPromise<T> : IUniTaskSource<T>
    {
        Action MoveNext { get; }
        UniTask<T> Task { get; }
        void SetResult(T result);
        void SetException(Exception exception);
    }

    internal sealed class AsyncUniTaskVoid<TStateMachine> : IStateMachineRunner, ITaskPoolNode<AsyncUniTaskVoid<TStateMachine>>, IUniTaskSource
        where TStateMachine : IAsyncStateMachine
    {
        static TaskPool<AsyncUniTaskVoid<TStateMachine>> pool;

        TStateMachine stateMachine;

        public Action MoveNext { get; }

        public AsyncUniTaskVoid()
        {
            MoveNext = Run;
        }

        public static void SetStateMachine(ref AsyncUniTaskVoidMethodBuilder builder, ref TStateMachine stateMachine)
        {
            if (!pool.TryPop(out var result))
            {
                result = new AsyncUniTaskVoid<TStateMachine>();
            }
            TaskTracker.TrackActiveTask(result, 3);

            builder.runner = result; // set runner before copied.
            result.stateMachine = stateMachine; // copy struct StateMachine(in release build).
        }

        static AsyncUniTaskVoid()
        {
            TaskPool.RegisterSizeGetter(typeof(AsyncUniTaskVoid<TStateMachine>), () => pool.Size);
        }

        public AsyncUniTaskVoid<TStateMachine> NextNode { get; set; }

        public void Return()
        {
            TaskTracker.RemoveTracking(this);
            stateMachine = default;
            pool.TryPush(this);
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Run()
        {
            stateMachine.MoveNext();
        }

        // dummy interface implementation for TaskTracker.

        UniTaskStatus IUniTaskSource.GetStatus(short token)
        {
            return UniTaskStatus.Pending;
        }

        UniTaskStatus IUniTaskSource.UnsafeGetStatus()
        {
            return UniTaskStatus.Pending;
        }

        void IUniTaskSource.OnCompleted(Action<object> continuation, object state, short token)
        {
        }

        void IUniTaskSource.GetResult(short token)
        {
        }
    }

    internal sealed class AsyncUniTask<TStateMachine> : IStateMachineRunnerPromise, IUniTaskSource, ITaskPoolNode<AsyncUniTask<TStateMachine>>
        where TStateMachine : IAsyncStateMachine
    {
        static TaskPool<AsyncUniTask<TStateMachine>> pool;

        TStateMachine stateMachine;

        public Action MoveNext { get; }

        UniTaskCompletionSourceCore<AsyncUnit> core;

        AsyncUniTask()
        {
            MoveNext = Run;
        }

        public static void SetStateMachine(ref AsyncUniTaskMethodBuilder builder, ref TStateMachine stateMachine)
        {
            if (!pool.TryPop(out var result))
            {
                result = new AsyncUniTask<TStateMachine>();
            }
            TaskTracker.TrackActiveTask(result, 3);

            builder.runnerPromise = result; // set runner before copied.
            result.stateMachine = stateMachine; // copy struct StateMachine(in release build).
        }

        public AsyncUniTask<TStateMachine> NextNode { get; set; }

        static AsyncUniTask()
        {
            TaskPool.RegisterSizeGetter(typeof(AsyncUniTask<TStateMachine>), () => pool.Size);
        }

        bool TryReturn()
        {
            TaskTracker.RemoveTracking(this);
            core.Reset();
            stateMachine = default;
            return pool.TryPush(this);
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Run()
        {
            stateMachine.MoveNext();
        }

        public UniTask Task
        {
            [DebuggerHidden]
            get
            {
                return new UniTask(this, core.Version);
            }
        }

        [DebuggerHidden]
        public void SetResult()
        {
            core.TrySetResult(AsyncUnit.Default);
        }

        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            core.TrySetException(exception);
        }

        [DebuggerHidden]
        public void GetResult(short token)
        {
            try
            {
                core.GetResult(token);
            }
            finally
            {
                TryReturn();
            }
        }

        [DebuggerHidden]
        public UniTaskStatus GetStatus(short token)
        {
            return core.GetStatus(token);
        }

        [DebuggerHidden]
        public UniTaskStatus UnsafeGetStatus()
        {
            return core.UnsafeGetStatus();
        }

        [DebuggerHidden]
        public void OnCompleted(Action<object> continuation, object state, short token)
        {
            core.OnCompleted(continuation, state, token);
        }

        ~AsyncUniTask()
        {
            if (TryReturn())
            {
                GC.ReRegisterForFinalize(this);
            }
        }
    }

    internal sealed class AsyncUniTask<TStateMachine, T> : IStateMachineRunnerPromise<T>, IUniTaskSource<T>, ITaskPoolNode<AsyncUniTask<TStateMachine, T>>
        where TStateMachine : IAsyncStateMachine
    {
        static TaskPool<AsyncUniTask<TStateMachine, T>> pool;

        TStateMachine stateMachine;

        public Action MoveNext { get; }

        UniTaskCompletionSourceCore<T> core;

        AsyncUniTask()
        {
            MoveNext = Run;
        }

        public static void SetStateMachine(ref AsyncUniTaskMethodBuilder<T> builder, ref TStateMachine stateMachine)
        {
            if (!pool.TryPop(out var result))
            {
                result = new AsyncUniTask<TStateMachine, T>();
            }
            TaskTracker.TrackActiveTask(result, 3);

            builder.runnerPromise = result; // set runner before copied.
            result.stateMachine = stateMachine; // copy struct StateMachine(in release build).
        }

        public AsyncUniTask<TStateMachine, T> NextNode { get; set; }

        static AsyncUniTask()
        {
            TaskPool.RegisterSizeGetter(typeof(AsyncUniTask<TStateMachine, T>), () => pool.Size);
        }

        bool TryReturn()
        {
            TaskTracker.RemoveTracking(this);
            core.Reset();
            stateMachine = default;
            return pool.TryPush(this);
        }

        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Run()
        {
            stateMachine.MoveNext();
        }

        public UniTask<T> Task
        {
            [DebuggerHidden]
            get
            {
                return new UniTask<T>(this, core.Version);
            }
        }

        [DebuggerHidden]
        public void SetResult(T result)
        {
            core.TrySetResult(result);
        }

        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            core.TrySetException(exception);
        }

        [DebuggerHidden]
        public T GetResult(short token)
        {
            try
            {
                return core.GetResult(token);
            }
            finally
            {
                TryReturn();
            }
        }

        [DebuggerHidden]
        void IUniTaskSource.GetResult(short token)
        {
            GetResult(token);
        }

        [DebuggerHidden]
        public UniTaskStatus GetStatus(short token)
        {
            return core.GetStatus(token);
        }

        [DebuggerHidden]
        public UniTaskStatus UnsafeGetStatus()
        {
            return core.UnsafeGetStatus();
        }

        [DebuggerHidden]
        public void OnCompleted(Action<object> continuation, object state, short token)
        {
            core.OnCompleted(continuation, state, token);
        }

        ~AsyncUniTask()
        {
            if (TryReturn())
            {
                GC.ReRegisterForFinalize(this);
            }
        }
    }
}

