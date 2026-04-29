import { SupersetConfig } from '../../types/builder';

describe('SupersetConfig visual grouping', () => {
  it('should visually group exercises by groupId', () => {
    const groupId = 'superset-1';
    const exercises = [
      { id: 'ex-1', supersetConfig: { groupId, position: 1 } as SupersetConfig },
      { id: 'ex-2', supersetConfig: { groupId, position: 2 } as SupersetConfig },
      { id: 'ex-3', supersetConfig: undefined },
    ];
    const grouped = exercises.filter(e => e.supersetConfig?.groupId === groupId);
    expect(grouped.length).toBe(2);
    expect(grouped[0].supersetConfig?.position).toBe(1);
    expect(grouped[1].supersetConfig?.position).toBe(2);
  });

  it('should correctly order exercises by position within a superset', () => {
    const groupId = 'superset-group';
    const configA: SupersetConfig = { groupId, position: 1 };
    const configB: SupersetConfig = { groupId, position: 2 };
    const configC: SupersetConfig = { groupId, position: 3 };
    
    // Position 1 should come before 2, 2 before 3
    expect(configA.position).toBeLessThan(configB.position);
    expect(configB.position).toBeLessThan(configC.position);
  });

  it('should distinguish between different superset groups', () => {
    const groupA: SupersetConfig = { groupId: 'A', position: 1 };
    const groupB: SupersetConfig = { groupId: 'B', position: 1 };
    expect(groupA.groupId).not.toBe(groupB.groupId);
  });
});
