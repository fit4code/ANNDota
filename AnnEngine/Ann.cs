﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AnnEngine {
    public class Ann {
        private float[ ][ ][ ] _weights;
        private float[ ] _biasWeights;

        private const float MIN_START_WEIGHT = -0.5f;
        private const float MAX_START_WEIGHT = 0.5f;

        public float LearningSpeed;
        public float Moment;
        public Func<float, float> ActivationFunction;
        public readonly bool HasBiasNeurons;

        public Ann(uint inputNeurons, uint[ ] hiddenNeurons, uint outputNeurons, float learningSpeed,
            float moment, Func<float, float> activationFunction = null, bool useBiasNeurons = true) {
            LearningSpeed = learningSpeed;
            Moment = moment;
            ActivationFunction = activationFunction ?? Sigmoid;
            HasBiasNeurons = useBiasNeurons;
            _weights = new float[hiddenNeurons.Length + 1][ ][ ];
            _weights[0] = new float[inputNeurons][ ];
            for (uint i = 0; i < _weights[0].Length; i++) {
                _weights[0][i] = new float[hiddenNeurons[0]];
            }
            for (uint i = 0, iNext = 1; i < hiddenNeurons.Length; i++, iNext++) {
                _weights[iNext] = new float[hiddenNeurons[i]][ ];
                for (uint j = 0; j < hiddenNeurons[i]; j++) {
                    _weights[iNext][j] =
                        new float[(iNext == hiddenNeurons.Length) ? outputNeurons : hiddenNeurons[iNext]];
                }
            }
            for (uint i = 0; i < _weights.Length; i++) {
                for (uint j = 0; j < _weights[i].Length; j++) {
                    for (uint k = 0; k < _weights[i][j].Length; k++) {
                        _weights[i][j][k] = Utils.Random(-MIN_START_WEIGHT, MAX_START_WEIGHT);
                    }
                }
            }
            if (useBiasNeurons) {
                _biasWeights = new float[hiddenNeurons.Length + 1];
                for (uint i = 0; i < _biasWeights.Length; i++) {
                    _biasWeights[i] = Utils.Random(-MIN_START_WEIGHT, MAX_START_WEIGHT);
                }
            }
        }

        public float[ ] Run(float[ ] input) {
            // TODO: throw input.Length != input neurons count
            float[ ] values = new float[_weights[0].Length];
            for (uint i = 0; i < input.Length; i++) {
                values[i] = input[i];
            }
            for (uint currentLevel = 1, prevLevel = 0;
                currentLevel < _weights.Length + 1;
                currentLevel++, prevLevel++) {
                float[ ] newValues = new float[currentLevel == _weights.Length
                    ? _weights[prevLevel][0].Length
                    : _weights[currentLevel].Length];
                for (uint currentNeuron = 0; currentNeuron < newValues.Length; currentNeuron++) {
                    newValues[currentNeuron] = HasBiasNeurons ? -_biasWeights[prevLevel] : 0;
                    for (uint commingNeuron = 0; commingNeuron < _weights[prevLevel].Length; commingNeuron++) {
                        newValues[currentNeuron] +=
                            values[commingNeuron] * _weights[prevLevel][commingNeuron][currentNeuron];
                    }
                    newValues[currentNeuron] = ActivationFunction(newValues[currentNeuron]);
                }
                values = newValues;
            }
            return values;
        }

        public AnnResult Learn(float[ ] input, float[ ]idealResult) {
            // TODO: throw input.Length != input neurons count || idealResult.Length != output neurons count
            float[ ] result = Run(input);
            // Calculation the error
            float error = 0;
            for (uint i = 0; i < result.Length; i++) {
                error += (float) Math.Pow(idealResult[i] - result[i], 2);
            }
            error /= result.Length;
            return new AnnResult(result, error);
        }

        public static float Sigmoid(float x) => (float) (1f / (1f + Math.Exp(-x)));
    }
}