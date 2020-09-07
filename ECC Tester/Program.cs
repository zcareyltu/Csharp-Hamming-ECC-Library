using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC_Tester {
	class Program {
		static void Main(string[] args) {
			//Data to be sent
			int data = 0b1011001; //7 bits
			int nBits = 7;
			Console.WriteLine("Original:         {0}", Convert.ToString(data, 2));

			//Calculate the nom of bits required
			int m = nBits;
			int r = calcRedundantBits(m);
			Console.WriteLine("m = {0}", m);
			Console.WriteLine("r = {0}", r);

			//Determine the positions of redundant/data bits
			int arr = posRedundantBits(data, m, r);
			Console.WriteLine("Organized bits:   {0}", Convert.ToString(arr, 2));

			//Calculate the parity bits
			arr = calcParityBits(arr, m, r);
			Console.WriteLine("Data transferred: {0}", Convert.ToString(arr, 2));

			arr ^= 0b00000100000;
			Console.WriteLine("Error added:      {0}", Convert.ToString(arr, 2));

			int error = detectError(arr, m, r);
			Console.WriteLine("Error location:   {0}", error);

			if(error != 0) {
				arr ^= 1 << error;
			}
			Console.WriteLine("Corrected data:   {0}", Convert.ToString(arr, 2));

			int recv = decodeData(arr, m, r);
			Console.WriteLine("Data decoded:     {0}", Convert.ToString(recv, 2));
			/*int data2 = data;
			int xor = 0;
			for(int i = 0; i < nBits; i++) {
				if((data2 & 0x01) != 0) {
					xor ^= i;
				}
				data2 >>= 1;
			}*/

			//Console.WriteLine(arr);
			Console.WriteLine("Press Enter to continue.");
			Console.ReadLine();
		}

		static int detectError(int data, int m, int r) {
			int error = 0;
			int power = 8; //TODO
			for (int i = 0; i < r; i++) {
				int val = 0;
				int cpy = data >> 1;
				for (int j = 1; j <= (m + r); j++, cpy >>= 1) {
					if ((j & power) != 0) {
						val ^= (cpy & 0x01);
					}
				}

				//error |= (1 << power) * val;
				error = (error << 1) | val;
				power >>= 1; //or power *= 2;
			}

			return error;
		}

		static int decodeData(int data, int m, int r) {
			int recv = 0;
			int power = 1;
			data >>= 1; //Get rid of 0 index bit
			for(int i = 1; i <= (m + r); i++, data >>= 1) {
				if(i != power) {
					recv = (recv << 1) | (data & 0x01);
				} else {
					power *= 2;
				}
			}

			return recv;
		}

		static int calcParityBits(int arr, int m, int r) {
			int power = 0x01;
			for(int i = 0; i < r; i++) {
				int val = 0;
				int cpy = arr >> 1;
				for(int j = 1; j <= (m + r); j++, cpy >>= 1) {
					if((j & power) != 0) {
						val ^= (cpy & 0x01);
					}
				}

				arr |= (1 << power) * val;
				power <<= 1; //or power *= 2;
			}

			return arr;
		}

		static int posRedundantBits(int data, int m, int r) {
			//Redundancy bits are placed at the positions which correspond to the power of 2
			int j = (int)Math.Pow(2, r - 1);
			int result = 0;

			//If position is power of 2 then insert '0', else append the data
			for(int i = (m + r); i > 0; i--) {
				if(i == j) {
					r--;
					j = (int)Math.Pow(2, r - 1);
					result <<= 1;
				} else {
					result = (result << 1) | (data & 0x01);
					data >>= 1;
				}
			}

			return result << 1;
		}

		static int calcRedundantBits(int m) {
			for(int i = 0; i < m; i++) {
				if(Math.Pow(2, i) >= m + i + 1) {
					return i;
				}
			}
			return m;
		}
	}
}
