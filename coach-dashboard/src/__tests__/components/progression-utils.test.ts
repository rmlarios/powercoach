import {
  instancesToChartData,
  detectBlocks,
  getBlockTypeForPoint,
  getBlockColor,
  getBlockLabel,
  getTrendDirection,
  calculateSuggestedWeight,
  type ChartDataPoint,
} from '@/components/charts/progression-utils';
import type { ExerciseProgressionInstance } from '@/types/builder';

// ========================
// Mock Helpers
// ========================
function mockInstance(overrides: Partial<ExerciseProgressionInstance> = {}): ExerciseProgressionInstance {
  return {
    weekId: 'w-1',
    weekNumber: 1,
    dayId: 'd-1',
    dayName: 'Day 1',
    exerciseEntryId: 'ex-1',
    sets: 3,
    repsMin: 8,
    repsMax: 8,
    restSeconds: 180,
    ...overrides,
  };
}

function makePoint(weekNumber: number, percentageRM?: number, rpe?: number): ChartDataPoint {
  return {
    week: `S${weekNumber}`,
    weekNumber,
    percentageRM,
    rpe,
    volume: 24, // arbitrary
  };
}

// ========================
// instancesToChartData
// ========================
describe('instancesToChartData', () => {
  it('should return empty array for empty instances', () => {
    expect(instancesToChartData([])).toEqual([]);
  });

  it('should transform instances to chart data points', () => {
    const instances = [
      mockInstance({ weekNumber: 1, sets: 3, repsMin: 8, percentageRM: 65, rpeTarget: 7 }),
      mockInstance({ weekNumber: 2, sets: 3, repsMin: 6, percentageRM: 70, rpeTarget: 8 }),
    ];

    const result = instancesToChartData(instances);
    expect(result).toHaveLength(2);
    expect(result[0]).toEqual(expect.objectContaining({
      week: 'S1',
      weekNumber: 1,
      percentageRM: 65,
      rpe: 7,
      volume: 24, // 3*8
    }));
    expect(result[1]).toEqual(expect.objectContaining({
      week: 'S2',
      weekNumber: 2,
      percentageRM: 70,
      rpe: 8,
      volume: 18, // 3*6
    }));
  });

  it('should calculate weight when oneRM is provided', () => {
    const instances = [
      mockInstance({ weekNumber: 1, percentageRM: 80 }),
    ];

    // 1RM=200, 80% = 160, rounded to 2.5 = 160
    const result = instancesToChartData(instances, 200);
    expect(result[0].weight).toBe(160);
  });

  it('should round weight to nearest 2.5kg', () => {
    const instances = [
      mockInstance({ weekNumber: 1, percentageRM: 67 }),
    ];

    // 1RM=200, 67% = 134, nearest 2.5 = 135
    const result = instancesToChartData(instances, 200);
    expect(result[0].weight).toBe(135);
  });

  it('should not calculate weight when oneRM is not provided', () => {
    const instances = [
      mockInstance({ weekNumber: 1, percentageRM: 80 }),
    ];

    const result = instancesToChartData(instances);
    expect(result[0].weight).toBeUndefined();
  });

  it('should not calculate weight when percentageRM is missing', () => {
    const instances = [
      mockInstance({ weekNumber: 1, percentageRM: undefined }),
    ];

    const result = instancesToChartData(instances, 200);
    expect(result[0].weight).toBeUndefined();
  });

  it('should handle zero sets/reps as volume 0', () => {
    const instances = [
      mockInstance({ weekNumber: 1, sets: 0, repsMin: 0 }),
    ];

    const result = instancesToChartData(instances);
    expect(result[0].volume).toBe(0);
  });
});

// ========================
// getBlockTypeForPoint
// ========================
describe('getBlockTypeForPoint', () => {
  it('should return "hypertrophy" for %RM <= 70', () => {
    expect(getBlockTypeForPoint(makePoint(1, 60), null)).toBe('hypertrophy');
    expect(getBlockTypeForPoint(makePoint(1, 70), null)).toBe('hypertrophy');
  });

  it('should return "strength" for 70 < %RM <= 85', () => {
    expect(getBlockTypeForPoint(makePoint(1, 71), null)).toBe('strength');
    expect(getBlockTypeForPoint(makePoint(1, 85), null)).toBe('strength');
  });

  it('should return "peaking" for %RM > 85', () => {
    expect(getBlockTypeForPoint(makePoint(1, 86), null)).toBe('peaking');
    expect(getBlockTypeForPoint(makePoint(1, 95), null)).toBe('peaking');
  });

  it('should return "unknown" when %RM is not set', () => {
    expect(getBlockTypeForPoint(makePoint(1, undefined), null)).toBe('unknown');
  });

  it('should return "deload" when %RM drops more than 10 from previous', () => {
    const prev = makePoint(1, 85);
    const current = makePoint(2, 60); // drop of 25
    expect(getBlockTypeForPoint(current, prev)).toBe('deload');
  });

  it('should not return "deload" for small drops', () => {
    const prev = makePoint(1, 75);
    const current = makePoint(2, 70); // drop of 5 — normal progression
    expect(getBlockTypeForPoint(current, prev)).toBe('hypertrophy');
  });

  it('should not return "deload" when previous has no %RM', () => {
    const prev = makePoint(1, undefined);
    const current = makePoint(2, 60);
    expect(getBlockTypeForPoint(current, prev)).toBe('hypertrophy');
  });
});

// ========================
// detectBlocks
// ========================
describe('detectBlocks', () => {
  it('should return empty array for empty data', () => {
    expect(detectBlocks([])).toEqual([]);
  });

  it('should return single block for consistent data', () => {
    const data = [
      makePoint(1, 65),
      makePoint(2, 67),
      makePoint(3, 70),
    ];

    const blocks = detectBlocks(data);
    expect(blocks).toHaveLength(1);
    expect(blocks[0].type).toBe('hypertrophy');
    expect(blocks[0].startWeek).toBe(1);
    expect(blocks[0].endWeek).toBe(3);
  });

  it('should detect transition from hypertrophy to strength', () => {
    const data = [
      makePoint(1, 65),
      makePoint(2, 68),
      makePoint(3, 75), // transition here
      makePoint(4, 80),
    ];

    const blocks = detectBlocks(data);
    expect(blocks).toHaveLength(2);
    expect(blocks[0].type).toBe('hypertrophy');
    expect(blocks[0].endWeek).toBe(2);
    expect(blocks[1].type).toBe('strength');
    expect(blocks[1].startWeek).toBe(3);
  });

  it('should detect deload blocks', () => {
    const data = [
      makePoint(1, 80),
      makePoint(2, 82),
      makePoint(3, 85),
      makePoint(4, 60), // deload: drops >10 from 85
    ];

    const blocks = detectBlocks(data);
    const deloadBlock = blocks.find(b => b.type === 'deload');
    expect(deloadBlock).toBeDefined();
    expect(deloadBlock!.startWeek).toBe(4);
  });

  it('should detect a full periodization cycle', () => {
    // Hypertrophy → Strength → Peaking → Deload
    const data = [
      makePoint(1, 60),   // hypertrophy
      makePoint(2, 65),   // hypertrophy
      makePoint(3, 75),   // strength
      makePoint(4, 80),   // strength
      makePoint(5, 88),   // peaking
      makePoint(6, 92),   // peaking
      makePoint(7, 60),   // deload (drop from 92)
    ];

    const blocks = detectBlocks(data);
    const types = blocks.map(b => b.type);
    expect(types).toContain('hypertrophy');
    expect(types).toContain('strength');
    expect(types).toContain('peaking');
    expect(types).toContain('deload');
  });

  it('should handle single data point', () => {
    const blocks = detectBlocks([makePoint(1, 75)]);
    expect(blocks).toHaveLength(1);
    expect(blocks[0].startWeek).toBe(1);
    expect(blocks[0].endWeek).toBe(1);
    expect(blocks[0].type).toBe('strength');
  });

  it('should include color in each block', () => {
    const blocks = detectBlocks([makePoint(1, 60), makePoint(2, 65)]);
    expect(blocks[0].color).toBeTruthy();
    expect(typeof blocks[0].color).toBe('string');
  });
});

// ========================
// getBlockColor / getBlockLabel
// ========================
describe('getBlockColor', () => {
  it('should return distinct colors for each block type', () => {
    const colors = new Set([
      getBlockColor('hypertrophy'),
      getBlockColor('strength'),
      getBlockColor('peaking'),
      getBlockColor('deload'),
    ]);
    expect(colors.size).toBe(4);
  });

  it('should return a color for unknown type', () => {
    expect(getBlockColor('unknown')).toBeTruthy();
  });
});

describe('getBlockLabel', () => {
  it('should return human-readable labels', () => {
    expect(getBlockLabel('hypertrophy')).toBe('Hypertrophy');
    expect(getBlockLabel('strength')).toBe('Strength');
    expect(getBlockLabel('peaking')).toBe('Peaking');
    expect(getBlockLabel('deload')).toBe('Deload');
  });

  it('should return empty string for unknown', () => {
    expect(getBlockLabel('unknown')).toBe('');
  });
});

// ========================
// getTrendDirection
// ========================
describe('getTrendDirection', () => {
  it('should return "improving" for positive trendKg', () => {
    expect(getTrendDirection(5.0)).toBe('improving');
    expect(getTrendDirection(0.1)).toBe('improving');
  });

  it('should return "declining" for negative trendKg', () => {
    expect(getTrendDirection(-3.0)).toBe('declining');
    expect(getTrendDirection(-0.1)).toBe('declining');
  });

  it('should return "stable" for zero trendKg', () => {
    expect(getTrendDirection(0)).toBe('stable');
  });

  it('should return null for undefined', () => {
    expect(getTrendDirection(undefined)).toBeNull();
  });
});

// ========================
// calculateSuggestedWeight
// ========================
describe('calculateSuggestedWeight', () => {
  it('should calculate weight from 1RM and percentage', () => {
    // 200kg * 65% = 130kg
    expect(calculateSuggestedWeight(200, 65)).toBe(130);
  });

  it('should round to nearest 2.5kg by default', () => {
    // 200kg * 67% = 134 → 135 (nearest 2.5)
    expect(calculateSuggestedWeight(200, 67)).toBe(135);
    // 200kg * 73% = 146 → 145 (nearest 2.5)
    expect(calculateSuggestedWeight(200, 73)).toBe(145);
  });

  it('should round to nearest 5kg when specified', () => {
    // 200kg * 67% = 134 → 135 (nearest 5)
    expect(calculateSuggestedWeight(200, 67, 5)).toBe(135);
    // 200kg * 73% = 146 → 145 (nearest 5)
    expect(calculateSuggestedWeight(200, 73, 5)).toBe(145);
  });

  it('should round to nearest 1kg when specified', () => {
    // 315kg * 84% = 264.6 → 265
    expect(calculateSuggestedWeight(315, 84, 1)).toBe(265);
  });

  it('should handle 100% correctly', () => {
    expect(calculateSuggestedWeight(200, 100)).toBe(200);
  });

  it('should handle small percentages', () => {
    // 200kg * 50% = 100
    expect(calculateSuggestedWeight(200, 50)).toBe(100);
  });
});
