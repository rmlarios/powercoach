import {
  epley,
  brzycki,
  estimateOneRepMax,
  calculateWorkingWeight,
  estimatedRepsForPercentage,
  estimateRPE,
  formatWeight,
  convertWeight,
  calculateProgressiveWeights,
  extractPercentageFromNotation,
  formatWeightSuggestion,
} from '@/utils/weight-calculator';

describe('weight-calculator', () => {
  // ============================================
  // 1RM Estimation Formulas
  // ============================================
  describe('epley formula', () => {
    it('should return the same weight for 1 rep', () => {
      expect(epley(100, 1)).toBe(100);
    });

    it('should return the same weight for 0 or negative reps', () => {
      expect(epley(100, 0)).toBe(100);
      expect(epley(100, -1)).toBe(100);
    });

    it('should calculate 1RM for given weight and reps', () => {
      // 100kg x 10 reps: 100 * (1 + 10/30) = 100 * 1.333 = 133.33
      expect(epley(100, 10)).toBeCloseTo(133.33, 1);
    });

    it('should increase as reps increase', () => {
      const fiveReps = epley(100, 5);
      const tenReps = epley(100, 10);
      expect(tenReps).toBeGreaterThan(fiveReps);
    });
  });

  describe('brzycki formula', () => {
    it('should return the same weight for 1 rep', () => {
      expect(brzycki(100, 1)).toBe(100);
    });

    it('should return the same weight for 0 or negative reps', () => {
      expect(brzycki(100, 0)).toBe(100);
      expect(brzycki(100, -1)).toBe(100);
    });

    it('should handle edge case of 37+ reps', () => {
      // Should not divide by zero or negative
      const result = brzycki(100, 37);
      expect(result).toBeDefined();
      expect(isFinite(result)).toBe(true);
    });

    it('should calculate 1RM for given weight and reps', () => {
      // 100kg x 10 reps: 100 * 36 / (37 - 10) = 3600 / 27 = 133.33
      expect(brzycki(100, 10)).toBeCloseTo(133.33, 1);
    });
  });

  describe('estimateOneRepMax', () => {
    it('should return average of epley and brzycki', () => {
      const weight = 100;
      const reps = 5;
      const expected = (epley(weight, reps) + brzycki(weight, reps)) / 2;
      expect(estimateOneRepMax(weight, reps)).toBe(expected);
    });

    it('should return same weight for 1 rep', () => {
      expect(estimateOneRepMax(100, 1)).toBe(100);
    });
  });

  // ============================================
  // Working Weight Calculation
  // ============================================
  describe('calculateWorkingWeight', () => {
    it('should calculate correct percentage of 1RM', () => {
      // 100kg * 75% = 75kg (no rounding needed)
      expect(calculateWorkingWeight(100, 75, 0)).toBe(75);
    });

    it('should round to 2.5kg by default', () => {
      // 100kg * 77% = 77kg -> rounds to 77.5kg
      expect(calculateWorkingWeight(100, 77)).toBe(77.5);
      
      // 100kg * 73% = 73kg -> rounds to 72.5kg
      expect(calculateWorkingWeight(100, 73)).toBe(72.5);
    });

    it('should round to 5kg when specified', () => {
      // 100kg * 77% = 77kg -> rounds to 75kg
      expect(calculateWorkingWeight(100, 77, 5)).toBe(75);
      
      // 100kg * 78% = 78kg -> rounds to 80kg
      expect(calculateWorkingWeight(100, 78, 5)).toBe(80);
    });

    it('should round to 1kg when specified', () => {
      // 100kg * 77.3% = 77.3kg -> rounds to 77kg
      expect(calculateWorkingWeight(100, 77.3, 1)).toBe(77);
    });

    it('should not round when roundTo is 0', () => {
      expect(calculateWorkingWeight(100, 77.5, 0)).toBe(77.5);
    });

    it('should handle various percentages', () => {
      expect(calculateWorkingWeight(100, 100)).toBe(100);
      expect(calculateWorkingWeight(100, 50)).toBe(50);
      expect(calculateWorkingWeight(200, 75)).toBe(150);
    });
  });

  // ============================================
  // Rep/Percentage Relationships
  // ============================================
  describe('estimatedRepsForPercentage', () => {
    it('should return correct ranges for different percentages', () => {
      expect(estimatedRepsForPercentage(95)).toEqual({ min: 1, max: 2, optimal: 1 });
      expect(estimatedRepsForPercentage(87)).toEqual({ min: 2, max: 4, optimal: 3 });
      expect(estimatedRepsForPercentage(82)).toEqual({ min: 3, max: 5, optimal: 4 });
      expect(estimatedRepsForPercentage(77)).toEqual({ min: 4, max: 6, optimal: 5 });
      expect(estimatedRepsForPercentage(72)).toEqual({ min: 5, max: 8, optimal: 6 });
      expect(estimatedRepsForPercentage(67)).toEqual({ min: 6, max: 10, optimal: 8 });
      expect(estimatedRepsForPercentage(62)).toEqual({ min: 8, max: 12, optimal: 10 });
      expect(estimatedRepsForPercentage(55)).toEqual({ min: 10, max: 15, optimal: 12 });
    });
  });

  describe('estimateRPE', () => {
    it('should return lower RPE for fewer reps', () => {
      const oneRM = 100;
      const weight = 75; // 75%
      
      expect(estimateRPE(weight, oneRM, 1)).toBeLessThan(estimateRPE(weight, oneRM, 5));
    });

    it('should return 10 for reps near failure', () => {
      const oneRM = 100;
      const weight = 75; // 75% - optimal around 5 reps
      
      expect(estimateRPE(weight, oneRM, 8)).toBe(10);
    });
  });

  // ============================================
  // Formatting & Conversion
  // ============================================
  describe('formatWeight', () => {
    it('should format with kg unit by default', () => {
      expect(formatWeight(100)).toBe('100kg');
    });

    it('should format with lb unit when specified', () => {
      expect(formatWeight(100, 'lb')).toBe('100lb');
    });

    it('should round to 1 decimal place', () => {
      expect(formatWeight(100.123)).toBe('100.1kg');
      expect(formatWeight(100.789)).toBe('100.8kg');
    });
  });

  describe('convertWeight', () => {
    it('should return same weight when units match', () => {
      expect(convertWeight(100, 'kg', 'kg')).toBe(100);
      expect(convertWeight(100, 'lb', 'lb')).toBe(100);
    });

    it('should convert kg to lb', () => {
      // 100kg = 220.462lb
      expect(convertWeight(100, 'kg', 'lb')).toBeCloseTo(220.462, 2);
    });

    it('should convert lb to kg', () => {
      // 220.462lb = 100kg
      expect(convertWeight(220.462, 'lb', 'kg')).toBeCloseTo(100, 1);
    });
  });

  // ============================================
  // Progressive Weights
  // ============================================
  describe('calculateProgressiveWeights', () => {
    it('should calculate weights for a series of percentages', () => {
      const weights = calculateProgressiveWeights(100, [70, 75, 80, 85]);
      expect(weights).toEqual([70, 75, 80, 85]);
    });

    it('should round according to roundTo parameter', () => {
      const weights = calculateProgressiveWeights(100, [72, 77, 82, 87], 5);
      expect(weights).toEqual([70, 75, 80, 85]);
    });

    it('should handle empty array', () => {
      expect(calculateProgressiveWeights(100, [])).toEqual([]);
    });
  });

  // ============================================
  // Notation Parsing
  // ============================================
  describe('extractPercentageFromNotation', () => {
    it('should extract percentage from notation with %', () => {
      expect(extractPercentageFromNotation('3x5 @75%')).toBe(75);
      expect(extractPercentageFromNotation('3x5 75%')).toBe(75);
      expect(extractPercentageFromNotation('75%')).toBe(75);
    });

    it('should handle decimal percentages', () => {
      expect(extractPercentageFromNotation('3x5 @72.5%')).toBe(72.5);
    });

    it('should return null when no percentage is found', () => {
      expect(extractPercentageFromNotation('3x5')).toBeNull();
      expect(extractPercentageFromNotation('3x5 @8')).toBeNull();
      expect(extractPercentageFromNotation('')).toBeNull();
    });
  });

  describe('formatWeightSuggestion', () => {
    it('should return formatted weight suggestion', () => {
      expect(formatWeightSuggestion(100, 75)).toBe('75kg');
    });

    it('should apply rounding', () => {
      expect(formatWeightSuggestion(100, 77, 2.5)).toBe('77.5kg');
      expect(formatWeightSuggestion(100, 77, 5)).toBe('75kg');
    });

    it('should return null when oneRepMax is null', () => {
      expect(formatWeightSuggestion(null, 75)).toBeNull();
    });

    it('should return null when percentage is null', () => {
      expect(formatWeightSuggestion(100, null)).toBeNull();
    });
  });
});
