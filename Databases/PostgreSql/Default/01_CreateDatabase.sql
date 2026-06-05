-- language=PostgreSQL

CREATE SCHEMA peachtreebus;

-- reference definition of a pending queue messages table.
-- a specifically named version of this table will need to exist for each queue in use.
CREATE TABLE peachtreebus.queuename_pending
(
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    message_id uuid NOT NULL,
    priority integer NOT NULL,
    not_before timestamptz NOT NULL,
    enqueued timestamptz NOT NULL,
    completed timestamptz NULL,
    failed timestamptz NULL,
    retries smallint NOT NULL DEFAULT 0,
    headers text NOT NULL,
    body text NOT NULL
);

-- this index helps find the next message to process 
CREATE INDEX idx_queuename_pending_getnext
    ON peachtreebus.queuename_pending (priority DESC)
    INCLUDE (not_before);

-- this index is useful for the management tools.
CREATE INDEX idx_qeuename_pending_enqueued
    ON peachtreebus.queuename_pending (enqueued DESC);

-- reference definition of a completed queue messages table.
-- a specifically named version of this table will need to exist for each queue in use.
CREATE TABLE peachtreebus.queuename_completed
(
    id bigint PRIMARY KEY NOT NULL,
    message_id uuid NOT NULL,
    priority integer NOT NULL,
    not_before timestamptz NOT NULL,
    enqueued timestamptz NOT NULL,
    completed timestamptz NULL,
    failed timestamptz NULL,
    retries smallint NOT NULL DEFAULT 0,
    headers text NOT NULL,
    body text NOT NULL
);

-- this index is useful for management tools.
CREATE INDEX idx_queuename_completed_enqueued
    ON peachtreebus.queuename_completed (enqueued DESC);

-- reference definition of a failed queue messages table.
-- a specifically named version of this table will need to exist for each queue in use.
CREATE TABLE peachtreebus.queuename_failed
(
    id bigint PRIMARY KEY NOT NULL,
    message_id uuid NOT NULL,
    priority integer NOT NULL,
    not_before timestamptz NOT NULL,
    enqueued timestamptz NOT NULL,
    completed timestamptz NULL,
    failed timestamptz NULL,
    retries smallint NOT NULL DEFAULT 0,
    Headers text NOT NULL,
    Body text NOT NULL
);

-- this index is useful for management tools.
CREATE INDEX idx_queueame_failed_enqueued
    ON peachtreebus.queuename_failed (enqueued DESC);

-- reference definition of a saga data table. A specifically named table
-- will need to exist for each saga defined in code.
CREATE TABLE peachtreebus.saganame_sagadata
(
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY NOT NULL,
    saga_id uuid NOT NULL,
    key varchar(128) UNIQUE NOT NULL,
    data text NOT NULL,
    meta_data text NOT NULL
);

-- Reference definition for the Subscriptions table.
CREATE TABLE peachtreebus.subscriptions
(
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY NOT NULL,
    subscriber_id uuid NOT NULL,
    topic varchar(128) NOT NULL,
    valid_until timestamptz NOT NULL
);

CREATE UNIQUE INDEX idx_subscriptions_subscriber_topic
    ON peachtreebus.subscriptions (subscriber_id, topic);

CREATE INDEX idx_subscriptions_valid_until_topic
    ON peachtreebus.subscriptions (valid_until, topic);

-- reference definition of a subscribed messages table.
-- a specifically named version of this table will need to exist for each queue in use.
CREATE TABLE peachtreebus.subscribed_pending
(
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    subscriber_id uuid NOT NULL,
    topic VARCHAR(128) NOT NULL,
    valid_until timestamptz NOT NULL,
    message_id uuid NOT NULL,
    priority integer NOT NULL,
    not_before timestamptz NOT NULL,
    enqueued timestamptz NOT NULL,
    completed timestamptz NULL,
    failed timestamptz NULL,
    retries smallint NOT NULL DEFAULT 0,
    headers TEXT NOT NULL,
    body TEXT NOT NULL
);

CREATE INDEX idx_subscribed_pending_next_message
    ON peachtreebus.subscribed_pending (subscriber_id, priority DESC)
    INCLUDE (not_before);

CREATE INDEX idx_subscribed_pending_enqueued
    ON peachtreeBus.subscribed_pending (enqueued);

-- reference definition of a subscribed messages table.
-- a specifically named version of this table will need to exist for each queue in use.
CREATE TABLE peachtreebus.subscribed_completed
(
    id bigint PRIMARY KEY NOT NULL,
    subscriber_id uuid NOT NULL,
    topic VARCHAR(128) NOT NULL,
    valid_until timestamptz NOT NULL,
    message_id uuid NOT NULL,
    priority integer NOT NULL,
    not_before timestamptz NOT NULL,
    enqueued timestamptz NOT NULL,
    completed timestamptz NULL,
    failed timestamptz NULL,
    retries smallint NOT NULL DEFAULT 0,
    headers TEXT NOT NULL,
    body TEXT NOT NULL
);

CREATE INDEX idx_subscribed_completed_enqueued
    ON peachtreeBus.subscribed_completed (enqueued);

-- reference definition of a subscribed messages table.
-- a specifically named version of this table will need to exist for each queue in use.
CREATE TABLE peachtreebus.subscribed_failed
(
    id bigint PRIMARY KEY NOT NULL,
    subscriber_id uuid NOT NULL,
    topic VARCHAR(128) NOT NULL,
    valid_until timestamptz NOT NULL,
    message_id uuid NOT NULL,
    priority integer NOT NULL,
    not_before timestamptz NOT NULL,
    enqueued timestamptz NOT NULL,
    completed timestamptz NULL,
    failed timestamptz NULL,
    retries smallint NOT NULL DEFAULT 0,
    headers TEXT NOT NULL,
    body TEXT NOT NULL
);

CREATE INDEX idx_subscribed_failed_enqueued
    ON peachtreeBus.subscribed_failed (enqueued);