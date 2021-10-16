using PeachtreeBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peachtreebus.Tests
{
    public class TestSagaMessage1: IMessage
    {

    }

    public class TestSagaMessage2: IMessage
    {

    }

    public class TestSagaData
    {

    }

    public class TestSaga : Saga<TestSagaData>
    {
        public override string SagaName => "TestSaga";

        public override void ConfigureMessageKeys(SagaMessageMap mapper)
        {
            throw new NotImplementedException();
        }
    }
}
