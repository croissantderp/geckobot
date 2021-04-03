namespace Quantum.qeckoBot {

    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Convert;
    open Microsoft.Quantum.Math;
    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Arrays;
    open Microsoft.Quantum.Measurement;
    open Microsoft.Quantum.Diagnostics;
    open Microsoft.Quantum.Oracles;
    open Microsoft.Quantum.AmplitudeAmplification;
	
    operation UnfairCoinFlip (flip : Bool) : Result {
        
		use qb = Qubit();
		H(qb);

		if (flip){
			X(qb);
		}

		H(qb);

		return MResetZ(qb);
    }

	//method: true is rect., false is diag.
    operation CoinFlip (input : Bool[], method : Bool, deMethod : Bool[]) : Result[] {
        
		use qubits = Qubit[5];
		
		//flips qubits to the correct positions
		for index in 0 .. Length(input) - 1 {
			
			if (input[index]){
				X(qubits[index]);
			}
		}

		//if diagonal method is being used, qubits are rotated 90 degrees
		if (method){
			
			ApplyToEach(H, qubits);
		}

		//decodes using specified methods
		for index in 0 .. Length(input) - 1{
				if (deMethod[index]){
					H(qubits[index]);
				}
			}

		//:lemonthink:
		mutable lemon = new Result[Length(input)];

		//collapses the qubits
		for index in 0 .. Length(input) - 1{
			set lemon w/= index <- M(qubits[index]);
		}

		ResetAll(qubits);
		
		return lemon;
    }
}
