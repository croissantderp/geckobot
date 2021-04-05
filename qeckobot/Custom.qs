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

    operation COperation (qubitcount : Int, arguments : Double[], operations : String[]) : Result[] {
        
		use qubits = Qubit[qubitcount];

		mutable index = 0;
		
		//:lemonthink:
		mutable lemon = new Result[qubitcount];

		for a in operations{

			if (a == "H"){
				H(qubits[Truncate(arguments[index])]);
			}
			elif (a == "I"){
				I(qubits[Truncate(arguments[index])]);
			}
			elif (a == "X"){
				X(qubits[Truncate(arguments[index])]);
			}
			elif (a == "Y"){
				Y(qubits[Truncate(arguments[index])]);
			}
			elif (a == "Z"){
				Z(qubits[Truncate(arguments[index])]);
			}
			elif (a == "T"){
				T(qubits[Truncate(arguments[index])]);
			}
			elif (a == "RX"){
				Rx(arguments[index], qubits[Truncate(arguments[index + 1])]);
				set index = index + 1;
			}
			elif (a == "RY"){
				Ry(arguments[index], qubits[Truncate(arguments[index + 1])]);
				set index = index + 1;
			}
			elif (a == "RZ"){
				Rz(arguments[index], qubits[Truncate(arguments[index + 1])]);
				set index = index + 1;
			}
			elif (a == "S"){
				S(qubits[Truncate(arguments[index])]);
			}
			elif (a == "SWAP"){
				SWAP(qubits[Truncate(arguments[index])], qubits[Truncate(arguments[index + 1])]);
				set index = index + 1;
			}
			elif (a == "CNOT"){
				CNOT(qubits[Truncate(arguments[index])], qubits[Truncate(arguments[index + 1])]);
				set index = index + 1;
			}
			elif (a == "CY"){
				CY(qubits[Truncate(arguments[index])], qubits[Truncate(arguments[index + 1])]);
				set index = index + 1;
			}
			elif (a == "CZ"){
				CZ(qubits[Truncate(arguments[index])], qubits[Truncate(arguments[index + 1])]);
				set index = index + 1;
			}
			elif (a == "CY"){
				CY(qubits[Truncate(arguments[index])], qubits[Truncate(arguments[index + 1])]);
				set index = index + 1;
			}
			elif (a == "CCNOT"){
				CCNOT(qubits[Truncate(arguments[index])], qubits[Truncate(arguments[index + 1])], qubits[Truncate(arguments[index + 2])]);
				set index = index + 2;
			}
			elif (a == "M"){
				set lemon w/= Truncate(arguments[index]) <- M(qubits[Truncate(arguments[index])]);
			}
			
			set index = index + 1;
		}

		ResetAll(qubits);
		return lemon;
    }
}
