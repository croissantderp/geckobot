namespace Quantum.qeckoBot {
	open Microsoft.Quantum.Convert;
    open Microsoft.Quantum.Math;
    open Microsoft.Quantum.Measurement;
    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Intrinsic;

    operation IsBlackBoxConstant (blackBox: ((Qubit, Qubit) => Unit)) : Bool
	{
        mutable inputResult = Zero;
		mutable outputResult = Zero;

		use qbits = Qubit[2];
			
		let input = qbits[0];
		let output = qbits[1];

		X(input);
		X(output);
		H(input);
		H(output);

		blackBox(input, output);

		H(input);
		H(output);

		set inputResult = M(input);
		set outputResult = M(output);


		return One == inputResult;
    }

	operation ConstantZero(input: Qubit, output: Qubit) : Unit
	{
	}
	operation ConstantOne(input: Qubit, output: Qubit) : Unit
	{
		X(output);
	}
	operation Identity(input: Qubit, output: Qubit) : Unit
	{
		CNOT(input, output);
	}
	operation Negation(input: Qubit, output: Qubit) : Unit
	{
		CNOT(input, output);
		X(output);
	}

	operation IsConstantZeroConstant() : Bool
	{
		return IsBlackBoxConstant(ConstantZero);
	}
	operation IsConstantOneConstant() : Bool
	{
		return IsBlackBoxConstant(ConstantOne);
	}
	operation IsIdentityConstant() : Bool
	{
		return IsBlackBoxConstant(Identity);
	}
	operation IsNegationConstant() : Bool
	{
		return IsBlackBoxConstant(Negation);
	}
}
