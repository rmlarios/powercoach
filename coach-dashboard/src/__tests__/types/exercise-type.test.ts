import { ExerciseType, DaySlotExercise, SupersetConfig, WeekPrescription } from '../../types/builder';

describe('ExerciseType', () => {
  it('should accept valid exercise type values', () => {
    const types: ExerciseType[] = ['standard', 'emom', 'tempo', 'superset', 'circuit', 'dropset'];
    expect(types).toContain('standard');
    expect(types).toContain('emom');
    expect(types).toContain('tempo');
    expect(types).toContain('superset');
    expect(types).toContain('circuit');
    expect(types).toContain('dropset');
  });

  it('should use ExerciseType in type annotations', () => {
    const standardType: ExerciseType = 'standard';
    const supersetType: ExerciseType = 'superset';
    expect(standardType).toBe('standard');
    expect(supersetType).toBe('superset');
  });
});

describe('SupersetConfig', () => {
  it('should create a valid superset config', () => {
    const config: SupersetConfig = {
      groupId: 'group-1',
      position: 1,
    };
    expect(config.groupId).toBe('group-1');
    expect(config.position).toBe(1);
  });

  it('should assign superset config to DaySlotExercise', () => {
    const weekPrescriptions = new Map<number, WeekPrescription>();
    weekPrescriptions.set(1, {
      weekId: 'week-1',
      dayId: 'day-1',
      exerciseEntryId: 'entry-1',
      sets: 3,
      reps: '3x8',
    });

    const exercise: DaySlotExercise = {
      exerciseId: 'ex-1',
      exerciseName: 'Bench Press',
      order: 1,
      weekPrescriptions,
      exerciseType: 'superset',
      supersetConfig: {
        groupId: 'group-2',
        position: 2,
      },
    };
    expect(exercise.exerciseType).toBe('superset');
    expect(exercise.supersetConfig?.groupId).toBe('group-2');
    expect(exercise.supersetConfig?.position).toBe(2);
  });
});
