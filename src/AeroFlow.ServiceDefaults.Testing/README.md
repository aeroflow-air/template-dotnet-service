# AeroFlow.ServiceDefaults.Testing

Contract-test helper for [AeroFlow.ServiceDefaults](../AeroFlow.ServiceDefaults/README.md). Register `LogCollector` on the test host's logging, then assert platform logs with `ObservabilityLogAssertions` (`RequireHttpRequest`, `RequireRequiredFields`, `RequireFlightId`).
