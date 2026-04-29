'use client';

import React, { useState } from 'react';
import {
  Dumbbell,
  Timer,
  Clock,
  Layers,
  RotateCw,
  TrendingDown,
  ChevronDown,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { Badge } from '@/components/ui/badge';
import {
  ExerciseType,
  EMOMConfig,
  TempoConfig,
  SupersetConfig,
  EXERCISE_TYPE_CONFIG,
} from '@/types/builder';
import { cn } from '@/utils/cn';
import { v4 as uuidv4 } from 'uuid';

// Icon mapping
const ICONS: Record<string, React.ComponentType<{ className?: string }>> = {
  Dumbbell,
  Timer,
  Clock,
  Layers,
  RotateCw,
  TrendingDown,
};

interface ExerciseTypeConfig {
  emomConfig?: EMOMConfig;
  tempoConfig?: TempoConfig;
  supersetConfig?: SupersetConfig;
}

interface ExerciseTypeSelectorProps {
  value: ExerciseType;
  emomConfig?: EMOMConfig;
  tempoConfig?: TempoConfig;
  supersetConfig?: SupersetConfig;
  /** Available superset groups to join */
  availableSupersetGroups?: { groupId: string; name: string; exerciseCount: number }[];
  onChange: (type: ExerciseType, config?: ExerciseTypeConfig) => void;
  compact?: boolean;
  /** Render inline without Popover wrapper (for use inside Dialog) */
  inline?: boolean;
}

export function ExerciseTypeSelector({
  value,
  emomConfig,
  tempoConfig,
  supersetConfig,
  availableSupersetGroups = [],
  onChange,
  compact = false,
  inline = false,
}: ExerciseTypeSelectorProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [localEmomConfig, setLocalEmomConfig] = useState<EMOMConfig>(
    emomConfig || { totalMinutes: 10, workSeconds: 40, restSeconds: 20 }
  );
  const [localTempoConfig, setLocalTempoConfig] = useState<TempoConfig>(
    tempoConfig || { eccentric: 3, pauseBottom: 1, concentric: 1, pauseTop: 0 }
  );
  const [localSupersetConfig, setLocalSupersetConfig] = useState<SupersetConfig>(
    supersetConfig || { groupId: '', position: 1 }
  );
  
  const currentConfig = EXERCISE_TYPE_CONFIG[value];
  const Icon = ICONS[currentConfig.icon];

  const handleTypeChange = (newType: ExerciseType) => {
    if (newType === 'emom') {
      onChange(newType, { emomConfig: localEmomConfig });
    } else if (newType === 'tempo') {
      onChange(newType, { tempoConfig: localTempoConfig });
    } else if (newType === 'superset') {
      // Create a new superset group if none selected
      const newGroupId = localSupersetConfig.groupId || uuidv4();
      const newConfig = { groupId: newGroupId, position: localSupersetConfig.position || 1 };
      setLocalSupersetConfig(newConfig);
      onChange(newType, { supersetConfig: newConfig });
    } else {
      onChange(newType);
      if (!inline) setIsOpen(false);
    }
  };

  const handleEmomConfigChange = (updates: Partial<EMOMConfig>) => {
    const newConfig = { ...localEmomConfig, ...updates };
    setLocalEmomConfig(newConfig);
    onChange('emom', { emomConfig: newConfig });
  };

  const handleTempoConfigChange = (updates: Partial<TempoConfig>) => {
    const newConfig = { ...localTempoConfig, ...updates };
    setLocalTempoConfig(newConfig);
    onChange('tempo', { tempoConfig: newConfig });
  };

  const handleSupersetConfigChange = (updates: Partial<SupersetConfig>) => {
    const newConfig = { ...localSupersetConfig, ...updates };
    setLocalSupersetConfig(newConfig);
    onChange('superset', { supersetConfig: newConfig });
  };

  const handleJoinSupersetGroup = (groupId: string) => {
    const existingGroup = availableSupersetGroups.find(g => g.groupId === groupId);
    const position = existingGroup ? existingGroup.exerciseCount + 1 : 1;
    const newConfig = { groupId, position };
    setLocalSupersetConfig(newConfig);
    onChange('superset', { supersetConfig: newConfig });
  };

  const handleCreateNewSupersetGroup = () => {
    const newGroupId = uuidv4();
    const newConfig = { groupId: newGroupId, position: 1 };
    setLocalSupersetConfig(newConfig);
    onChange('superset', { supersetConfig: newConfig });
  };

  // Inline content (for use inside Dialog)
  const selectorContent = (
    <>
      {/* Type Options */}
      <div className="grid grid-cols-2 gap-2">
        {(Object.keys(EXERCISE_TYPE_CONFIG) as ExerciseType[]).map((exerciseType) => {
          const config = EXERCISE_TYPE_CONFIG[exerciseType];
          const TypeIcon = ICONS[config.icon];
          const isSelected = value === exerciseType;

          return (
            <button
              key={exerciseType}
              onClick={() => handleTypeChange(exerciseType)}
              className={cn(
                'flex items-center gap-2 p-2 rounded-md text-left transition-colors',
                'hover:bg-muted/80 border',
                isSelected && 'bg-primary/10 ring-2 ring-primary/30 border-primary/40'
              )}
            >
              <div
                className={cn(
                  'w-8 h-8 rounded flex items-center justify-center flex-shrink-0',
                  isSelected ? 'bg-primary/20' : 'bg-muted'
                )}
              >
                <TypeIcon className={cn('h-4 w-4', isSelected ? 'text-primary' : 'text-muted-foreground')} />
              </div>
              <div className="flex-1 min-w-0">
                <div className="text-sm font-medium">{config.label}</div>
              </div>
            </button>
          );
        })}
      </div>

      {/* EMOM Configuration */}
      {value === 'emom' && (
        <div className="mt-4 p-3 rounded-lg bg-blue-50 border border-blue-200">
          <h5 className="text-sm font-medium mb-2 flex items-center gap-2">
            <Timer className="h-4 w-4 text-blue-600" />
            EMOM Configuration
          </h5>
          <div className="grid grid-cols-3 gap-2">
            <div>
              <Label className="text-xs">Minutes</Label>
              <Input
                type="number"
                min={1}
                max={60}
                value={localEmomConfig.totalMinutes}
                onChange={(e) =>
                  handleEmomConfigChange({
                    totalMinutes: parseInt(e.target.value) || 10,
                  })
                }
                className="h-8 text-sm"
              />
            </div>
            <div>
              <Label className="text-xs">Work (sec)</Label>
              <Input
                type="number"
                min={10}
                max={60}
                value={localEmomConfig.workSeconds || 40}
                onChange={(e) =>
                  handleEmomConfigChange({
                    workSeconds: parseInt(e.target.value) || 40,
                  })
                }
                className="h-8 text-sm"
              />
            </div>
            <div>
              <Label className="text-xs">Rest (sec)</Label>
              <Input
                type="number"
                min={0}
                max={60}
                value={localEmomConfig.restSeconds || 20}
                onChange={(e) =>
                  handleEmomConfigChange({
                    restSeconds: parseInt(e.target.value) || 20,
                  })
                }
                className="h-8 text-sm"
              />
            </div>
          </div>
        </div>
      )}

      {/* Tempo Configuration */}
      {value === 'tempo' && (
        <div className="mt-4 p-3 rounded-lg bg-purple-50 border border-purple-200">
          <h5 className="text-sm font-medium mb-2 flex items-center gap-2">
            <Clock className="h-4 w-4 text-purple-600" />
            Tempo Configuration
          </h5>
          <div className="grid grid-cols-4 gap-2">
            <div>
              <Label className="text-xs">Eccentric</Label>
              <Input
                type="number"
                min={0}
                max={10}
                value={localTempoConfig.eccentric}
                onChange={(e) =>
                  handleTempoConfigChange({
                    eccentric: parseInt(e.target.value) || 0,
                  })
                }
                className="h-8 text-sm text-center"
              />
            </div>
            <div>
              <Label className="text-xs">Pause ↓</Label>
              <Input
                type="number"
                min={0}
                max={10}
                value={localTempoConfig.pauseBottom}
                onChange={(e) =>
                  handleTempoConfigChange({
                    pauseBottom: parseInt(e.target.value) || 0,
                  })
                }
                className="h-8 text-sm text-center"
              />
            </div>
            <div>
              <Label className="text-xs">Concentric</Label>
              <Input
                type="number"
                min={0}
                max={10}
                value={localTempoConfig.concentric}
                onChange={(e) =>
                  handleTempoConfigChange({
                    concentric: parseInt(e.target.value) || 0,
                  })
                }
                className="h-8 text-sm text-center"
              />
            </div>
            <div>
              <Label className="text-xs">Pause ↑</Label>
              <Input
                type="number"
                min={0}
                max={10}
                value={localTempoConfig.pauseTop || 0}
                onChange={(e) =>
                  handleTempoConfigChange({
                    pauseTop: parseInt(e.target.value) || 0,
                  })
                }
                className="h-8 text-sm text-center"
              />
            </div>
          </div>
          <div className="mt-2 text-center">
            <Badge variant="secondary" className="font-mono text-base">
              {localTempoConfig.eccentric}:{localTempoConfig.pauseBottom}:
              {localTempoConfig.concentric}
              {localTempoConfig.pauseTop ? `:${localTempoConfig.pauseTop}` : ''}
            </Badge>
          </div>
        </div>
      )}

      {/* Superset Configuration */}
      {value === 'superset' && (
        <div className="mt-4 p-3 rounded-lg bg-orange-50 border border-orange-200">
          <h5 className="text-sm font-medium mb-2 flex items-center gap-2">
            <Layers className="h-4 w-4 text-orange-600" />
            Superset Configuration
          </h5>
          
          {availableSupersetGroups.length > 0 ? (
            <div className="space-y-2">
              <Label className="text-xs">Join existing superset or create new:</Label>
              <div className="space-y-1">
                {availableSupersetGroups.map((group) => (
                  <button
                    key={group.groupId}
                    onClick={() => handleJoinSupersetGroup(group.groupId)}
                    className={cn(
                      'w-full flex items-center justify-between p-2 rounded-md text-left text-sm',
                      'hover:bg-orange-100 border transition-colors',
                      localSupersetConfig.groupId === group.groupId && 'bg-orange-100 ring-2 ring-orange-300 border-orange-400'
                    )}
                  >
                    <span>{group.name}</span>
                    <Badge variant="secondary" className="text-xs">
                      {group.exerciseCount} exercise{group.exerciseCount !== 1 ? 's' : ''}
                    </Badge>
                  </button>
                ))}
              </div>
              <div className="pt-2 border-t">
                <Button
                  variant="outline"
                  size="sm"
                  className="w-full"
                  onClick={handleCreateNewSupersetGroup}
                >
                  + Create New Superset Group
                </Button>
              </div>
            </div>
          ) : (
            <div className="space-y-2">
              <p className="text-xs text-muted-foreground">
                This exercise will start a new superset group. Add another exercise to this superset by selecting it and choosing &quot;Superset&quot; type, then joining this group.
              </p>
              <div className="flex items-center gap-2">
                <Label className="text-xs">Position:</Label>
                <Input
                  type="number"
                  min={1}
                  max={10}
                  value={localSupersetConfig.position}
                  onChange={(e) =>
                    handleSupersetConfigChange({
                      position: parseInt(e.target.value) || 1,
                    })
                  }
                  className="h-8 w-16 text-sm text-center"
                />
              </div>
              {localSupersetConfig.groupId && (
                <Badge variant="secondary" className="text-xs">
                  Group ID: {localSupersetConfig.groupId.slice(0, 8)}...
                </Badge>
              )}
            </div>
          )}
        </div>
      )}
    </>
  );

  // Inline mode - just render the content
  if (inline) {
    return <div>{selectorContent}</div>;
  }

  // Popover mode (original behavior)
  return (
    <Popover open={isOpen} onOpenChange={setIsOpen}>
      <PopoverTrigger asChild>
        {compact ? (
          <button
            className={cn(
              'inline-flex items-center gap-1 px-1.5 py-0.5 rounded text-xs font-medium',
              'hover:bg-muted/80 transition-colors',
              value !== 'standard' && `bg-${currentConfig.color}-100 text-${currentConfig.color}-700`
            )}
          >
            <Icon className="h-3 w-3" />
            {value !== 'standard' && <span>{currentConfig.shortLabel}</span>}
          </button>
        ) : (
          <Button variant="outline" size="sm" className="gap-2">
            <Icon className="h-4 w-4" />
            <span>{currentConfig.label}</span>
            <ChevronDown className="h-3 w-3 opacity-50" />
          </Button>
        )}
      </PopoverTrigger>
      <PopoverContent className="w-80 p-3" align="start">
        {selectorContent}
      </PopoverContent>
    </Popover>
  );
}

/**
 * Compact badge display for exercise type (used in table rows)
 */
interface ExerciseTypeBadgeProps {
  type: ExerciseType;
  emomConfig?: EMOMConfig;
  tempoConfig?: TempoConfig;
  supersetConfig?: SupersetConfig;
  onClick?: () => void;
  /** Compact mode: icon-only with tooltip (for tight spaces) */
  compact?: boolean;
}

export function ExerciseTypeBadge({
  type,
  emomConfig,
  tempoConfig,
  supersetConfig,
  onClick,
  compact,
}: ExerciseTypeBadgeProps) {
  if (type === 'standard') return null;

  const config = EXERCISE_TYPE_CONFIG[type];
  const Icon = ICONS[config.icon];

  let extraInfo = '';
  if (type === 'emom' && emomConfig) {
    extraInfo = `${emomConfig.totalMinutes}min`;
  } else if (type === 'tempo' && tempoConfig) {
    extraInfo = `${tempoConfig.eccentric}:${tempoConfig.pauseBottom}:${tempoConfig.concentric}`;
  } else if (type === 'superset' && supersetConfig) {
    extraInfo = `#${supersetConfig.position}`;
  }

  const tooltipText = `${config.label}${extraInfo ? ` (${extraInfo})` : ''}`;

  return (
    <button
      onClick={onClick}
      title={tooltipText}
      className={cn(
        'inline-flex items-center gap-1 rounded font-medium flex-shrink-0',
        'hover:opacity-80 transition-opacity cursor-pointer',
        compact ? 'px-1 py-0.5 text-[10px]' : 'px-1.5 py-0.5 text-xs',
        type === 'emom' && 'bg-blue-100 text-blue-700',
        type === 'tempo' && 'bg-purple-100 text-purple-700',
        type === 'superset' && 'bg-orange-100 text-orange-700',
        type === 'circuit' && 'bg-green-100 text-green-700',
        type === 'dropset' && 'bg-red-100 text-red-700'
      )}
    >
      <Icon className={compact ? 'h-2.5 w-2.5' : 'h-3 w-3'} />
      {!compact && <span>{config.shortLabel}</span>}
      {!compact && extraInfo && <span className="opacity-75">({extraInfo})</span>}
    </button>
  );
}

/**
 * Format tempo to string notation
 */
export function formatTempo(config: TempoConfig): string {
  const parts = [config.eccentric, config.pauseBottom, config.concentric];
  if (config.pauseTop) parts.push(config.pauseTop);
  return parts.join(':');
}

/**
 * Parse tempo string to config
 */
export function parseTempo(tempo: string): TempoConfig | null {
  const parts = tempo.split(':').map((p) => parseInt(p.trim()));
  if (parts.length < 3 || parts.some(isNaN)) return null;
  return {
    eccentric: parts[0],
    pauseBottom: parts[1],
    concentric: parts[2],
    pauseTop: parts[3],
  };
}
