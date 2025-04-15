﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Pipelines;
using PeachtreeBus.Queues;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Pipeline;

public abstract class OutgoingPipelineInvokerFixtureBase<TInvoker, TInternalContext, TContext, TPipeline, TFactory, TQueueData>
    where TInvoker : OutgoingPipelineInvoker<TInternalContext, TContext, TPipeline, TFactory>
    where TContext : IOutgoingContext
    where TInternalContext : OutgoingContext<TQueueData>, TContext
    where TQueueData : QueueData
    where TPipeline : class, IPipeline<TContext>
    where TFactory : class, IPipelineFactory<TInternalContext, TContext, TPipeline>
{
    protected TInvoker _invoker = default!;
    protected Mock<IWrappedScope> _scope = new();
    protected TInternalContext _context = default!;
    protected Mock<TFactory> _factory = new();
    protected Mock<TPipeline> _pipeline = new();

    [TestInitialize]
    public void Initialize()
    {
        _scope.Reset();
        _factory.Reset();
        _pipeline.Reset();

        _scope.Setup(s => s.GetInstance<TFactory>())
            .Returns(() => _factory.Object);
        _factory.Setup(f => f.Build(It.IsAny<TInternalContext>()))
            .Returns(() => _pipeline.Object);

        _context = CreateContext();
        _invoker = CreateInvoker();
    }

    protected abstract TInvoker CreateInvoker();
    protected abstract TInternalContext CreateContext();

    [TestMethod]
    public async Task Given_ContextIsNull_When_Invoke_Then_Throws()
    {
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
            _invoker.Invoke(null!));
    }

    [TestMethod]
    public async Task When_Invoke_Then_PipelineIsBuiltAndInvoked()
    {
        await _invoker.Invoke(_context);
        _scope.Verify(s => s.GetInstance<TFactory>(), Times.Once);
        _factory.Verify(f => f.Build(_context), Times.Once);
        _pipeline.Verify(p => p.Invoke(_context), Times.Once);
    }


    [TestMethod]
    public async Task When_Invoke_Then_ContextIsSetupFirst()
    {
        _factory.Setup(f => f.Build(It.IsAny<TInternalContext>()))
            .Callback((TInternalContext c) =>
            {
                Assert.AreSame(c, _context);
                Assert.AreSame(_scope.Object, c.Scope);
            })
            .Returns(() => _pipeline.Object);

        _pipeline.Setup(f => f.Invoke(It.IsAny<TContext>()))
            .Callback((TContext c) =>
            {
                Assert.AreSame(c, _context);
                Assert.AreSame(_scope.Object, c.Scope);
            });

        await _invoker.Invoke(_context);
    }
}
