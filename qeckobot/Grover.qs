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

    /// # Summary
    /// This operation applies Grover's algorithm to search all possible inputs
    /// to an operation to find a particular marked state.
    
    operation SearchForMarkedInput(nQubits : Int, key : Bool[]) : Result[] {
        use qubits = Qubit[nQubits];
        // Initialize a uniform superposition over all possible inputs.
        PrepareUniform(qubits);
        // The search itself consists of repeatedly reflecting about the
        // marked state and our start state, which we can write out in Q#
        // as a for loop.
        for idxIteration in 0..NIterations(nQubits) - 1 {
            ReflectAboutMarked(qubits, key);
            ReflectAboutUniform(qubits);
        }
        // Measure and return the answer.
        return ForEach(MResetZ, qubits);
    }

    /// # Summary
    /// Returns the number of Grover iterations needed to find a single marked
    /// item, given the number of qubits in a register.
    operation NIterations(nQubits : Int) : Int {
        let nItems = 1 <<< nQubits; // 2^numQubits
        // compute number of iterations:
        let angle = ArcSin(1. / Sqrt(IntAsDouble(nItems)));
        let nIterations = Round(0.25 * PI() / angle - 0.5);
        return nIterations;
    }

	/// # Summary
    /// Reflects about the basis state marked by alternating zeros and ones.
    /// This operation defines what input we are trying to find in the main
    /// search.
    operation ReflectAboutMarked(inputQubits : Qubit[], key : Bool[]) : Unit {
        use outputQubit = Qubit();
        within {
            // We initialize the outputQubit to (|0⟩ - |1⟩) / √2,
            // so that toggling it results in a (-1) phase.
            X(outputQubit);
            H(outputQubit);

            // Flip the outputQubit for marked states.
            for index in 0 .. Length(key) - 1 {
				if (key[index]) {
					X(inputQubits[index]);
				}
			}
            //ApplyToEachA(X, inputQubits); //...2...
        } apply {
            Controlled X(inputQubits, outputQubit);
        }
    }

    /// # Summary
    /// Reflects about the uniform superposition state.
    operation ReflectAboutUniform(inputQubits : Qubit[]) : Unit {
        within {
            // Transform the uniform superposition to all-zero.
            Adjoint PrepareUniform(inputQubits);
            // Transform the all-zero state to all-ones
            PrepareAllOnes(inputQubits);
        } apply {
            // Now that we've transformed the uniform superposition to the
            // all-ones state, reflect about the all-ones state, then let
            // the within/apply block transform us back.
            ReflectAboutAllOnes(inputQubits);
        }
    }

    /// # Summary
    /// Reflects about the all-ones state.
    operation ReflectAboutAllOnes(inputQubits : Qubit[]) : Unit {
        Controlled Z(Most(inputQubits), Tail(inputQubits));
    }

    /// # Summary
    /// Given a register in the all-zeros state, prepares a uniform
    /// superposition over all basis states.
    operation PrepareUniform(inputQubits : Qubit[]) : Unit is Adj + Ctl {
        ApplyToEachCA(H, inputQubits);
    }

    /// # Summary
    /// Given a register in the all-zeros state, prepares an all-ones state
    /// by flipping every qubit.
    operation PrepareAllOnes(inputQubits : Qubit[]) : Unit is Adj + Ctl {
        ApplyToEachCA(X, inputQubits);
    }


	operation ApplyDatabaseOracleFromInts(markedElements : Int[], markedQubit : Qubit, databaseRegister : Qubit[]): Unit is Adj + Ctl {
        for markedElement in markedElements {
            ControlledOnInt(markedElement, X)(databaseRegister, markedQubit);
        }
    }

    operation _GroverStatePrepOracle(markedElements : Int[], idxMarkedQubit : Int, startQubits : Qubit[]): Unit is Adj + Ctl {
        let flagQubit = startQubits[idxMarkedQubit];
        let databaseRegister = Exclude([idxMarkedQubit], startQubits);

        ApplyToEachCA(H, databaseRegister);

        ApplyDatabaseOracleFromInts(markedElements, flagQubit, databaseRegister);
    }

    function GroverStatePrepOracle(markedElements : Int[]) : StateOracle {
        return StateOracle(_GroverStatePrepOracle(markedElements, _, _));
    }

    function GroverSearch(markedElements : Int[], nIterations : Int, idxMarkedQubit : Int)
    : (Qubit[] => Unit is Adj + Ctl) {
        return StandardAmplitudeAmplification(nIterations, GroverStatePrepOracle(markedElements), idxMarkedQubit);
    }

    operation ApplyGroverSearch(markedElements : Int[], nIterations : Int, nDatabaseQubits : Int) : (Result, Result[]) {
        
        use (markedQubit, databaseRegister) = (Qubit(), Qubit[nDatabaseQubits]);
        // Implement the quantum search algorithm.
        GroverSearch(markedElements, nIterations, 0)([markedQubit] + databaseRegister);

        // Measure the marked qubit. On success, this should be One.
        let resultSuccess = MResetZ(markedQubit);

        // Measure the state of the database register post-selected on
        // the state of the marked qubit and return the measurement results
        // of the algorithm.
        return (resultSuccess, ForEach(MResetZ, databaseRegister));
    }
}
