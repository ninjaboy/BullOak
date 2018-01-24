﻿
namespace BullOak.Repositories.EventStore.Test.Integration.StepDefinitions
{
    using BullOak.Repositories.EventStore.Test.Integration.Components;
    using BullOak.Repositories.EventStore.Test.Integration.Contexts;
    using FluentAssertions;
    using System;
    using TechTalk.SpecFlow;
    using Xunit;

    [Binding]
    internal class SaveEventsStreamSteps
    {
        private readonly InProcEventStoreIntegrationContext eventStoreContainer;
        private readonly EventGenerator eventGenerator;
        private readonly TestDataContext testDataContext;


        public SaveEventsStreamSteps(InProcEventStoreIntegrationContext eventStoreContainer,
            EventGenerator eventGenerator,
            TestDataContext testDataContext)
        {
            this.eventStoreContainer = eventStoreContainer ?? throw new ArgumentNullException(nameof(eventStoreContainer));
            this.eventGenerator = eventGenerator ?? throw new ArgumentNullException(nameof(eventGenerator));
            this.testDataContext = testDataContext ?? throw new ArgumentNullException(nameof(testDataContext));
        }

        [Given(@"a new stream")]
        public void GivenANewStream()
        {
            testDataContext.ResetStream();
        }

        [Given(@"an existing stream with (.*) events")]
        public void GivenAnExistingStreamWithEvents(int count)
        {
            testDataContext.ResetStream();
            eventStoreContainer.WriteEventsToStreamRaw(
                testDataContext.CurrentStreamInUse,
                eventGenerator.GenerateEvents(count));
        }


        [Given(@"(.*) new events")]
        public void GivenNewEvents(int eventsNumber)
        {
            var events = eventGenerator.GenerateEvents(eventsNumber);
            testDataContext.LastGeneratedEvents = events;
        }

        [When(@"I try to save the new events in the stream")]
        public void WhenITryToSaveTheNewEventsInTheStream()
        {
            testDataContext.RecordedException = Record.Exception(() =>
                eventStoreContainer.AppendEventsToStream(testDataContext.CurrentStreamInUse, testDataContext.LastGeneratedEvents).Wait());
        }

        [Then(@"the save process should succeed")]
        public void ThenTheSaveProcessShouldSucceed()
        {
            testDataContext.RecordedException.Should().BeNull();
        }

        [Then(@"there should be (.*) events in the stream")]
        public void ThenThereShouldBeEventsInTheStream(int count)
        {
            var recordedEvents = eventStoreContainer.ReadEventsFromStreamRaw(testDataContext.CurrentStreamInUse);
            recordedEvents.Length.Should().Be(count);
        }

    }
}
