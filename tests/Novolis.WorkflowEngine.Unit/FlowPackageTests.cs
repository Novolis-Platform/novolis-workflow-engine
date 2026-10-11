using Novolis.WorkflowEngine.Flow;

namespace Novolis.WorkflowEngine.Unit;

public sealed class FlowPackageTests
{
    [Test]
    public async Task Matching_output_and_input_types_are_accepted()
    {
        var output = new PortDescriptor(
            new PortId("value"),
            PortDirection.Output,
            FlowType.Scalar);
        var input = new PortDescriptor(
            new PortId("value"),
            PortDirection.Input,
            FlowType.Scalar);

        var result = FlowConnectionValidator.Default.Validate(output, input);

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.Diagnostics.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Incompatible_types_are_rejected_without_conversion()
    {
        var output = new PortDescriptor(
            new PortId("value"),
            PortDirection.Output,
            FlowType.Scalar);
        var input = new PortDescriptor(
            new PortId("value"),
            PortDirection.Input,
            FlowType.Vector3);

        var result = FlowConnectionValidator.Default.Validate(output, input);

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Diagnostics.Any(diagnostic =>
            diagnostic.Code == FlowDiagnosticCode.TypeMismatch)).IsTrue();
    }

    [Test]
    public async Task Input_cardinality_is_checked_explicitly()
    {
        var output = new PortDescriptor(
            new PortId("value"),
            PortDirection.Output,
            FlowType.Scalar);
        var input = new PortDescriptor(
            new PortId("value"),
            PortDirection.Input,
            FlowType.Scalar,
            PortCardinality.Single);

        var result = FlowConnectionValidator.Default.Validate(
            output,
            input,
            existingDestinationConnections: 1);

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Diagnostics.Any(diagnostic =>
            diagnostic.Code == FlowDiagnosticCode.CardinalityExceeded)).IsTrue();
    }

    [Test]
    public async Task Semantic_type_equality_includes_type_arguments()
    {
        var scalarField = FlowType.Field(FlowType.Scalar);
        var vectorField = FlowType.Field(FlowType.Vector3);

        await Assert.That(scalarField.Equals(new FlowType("Field", [FlowType.Scalar])))
            .IsTrue();
        await Assert.That(scalarField.Equals(vectorField)).IsFalse();
    }
}
