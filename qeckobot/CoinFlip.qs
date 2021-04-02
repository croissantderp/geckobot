namespace Quantum.qeckoBot {

    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Intrinsic;
    open Microsoft.Quantum.Measurement;
	
    operation UnfairCoinFlip (flip : Bool) : Result {
        
		use qb = Qubit();
		H(qb);

		if (flip){
			X(qb);
		}

		H(qb);

		return MResetZ(qb);
    }

    operation CoinFlip () : Unit {
        
		use qubits = Qubit[5];

		ApplyToEach(H, qubits);



    }
}
