/**
 * Smart Set Notation Parser
 * 
 * Parses various workout set notations used by coaches:
 * 
 * Basic:
 *   "3x8"         → 3 sets of 8 reps
 *   "4x8-12"      → 4 sets of 8-12 reps (range)
 *   "3x10 @8"     → 3 sets of 10 reps at RPE 8
 * 
 * Compound:
 *   "1x1 3x4"     → 1 single + 3 sets of 4 (backoff)
 *   "1x1 3x4 84% 74%"  → With percentages per group
 * 
 * Special:
 *   "Single @9"   → 1 rep at RPE 9
 *   "AMRAP"       → As Many Reps As Possible
 *   "2x10 EMOM 6" → EMOM of 6 minutes
 * 
 * With tempo:
 *   "3x8 TEMPO 3:1:0" → With tempo specified
 * 
 * With direct weight:
 *   "3x5 @225"    → 3 sets of 5 at 225kg/lb
 */

export interface SetGroup {
  sets: number;
  repsMin: number;
  repsMax?: number;        // For ranges like 8-12
  percentage?: number;     // %RM
  rpe?: number;
  weight?: number;         // Direct weight
  tempo?: string;          // "3:1:0"
  isAMRAP?: boolean;
}

export interface ParsedNotation {
  groups: SetGroup[];
  emomMinutes?: number;
  tempo?: string;
  notes?: string;
  raw: string;
  isValid: boolean;
  error?: string;
}

// Regex patterns for parsing
const PATTERNS = {
  // Basic: "3x8" or "3x8-12"
  basic: /^(\d+)x(\d+)(?:-(\d+))?$/i,
  
  // With RPE: "3x8 @8" or "3x8@8"
  withRpe: /^(\d+)x(\d+)(?:-(\d+))?\s*@(\d+(?:\.\d+)?)$/i,
  
  // With percentage: "3x8 65%" or "3x8 @65%"
  withPercentage: /^(\d+)x(\d+)(?:-(\d+))?\s*@?(\d+(?:\.\d+)?)\s*%$/i,
  
  // With weight: "3x5 @225" (weight without %)
  withWeight: /^(\d+)x(\d+)(?:-(\d+))?\s*@(\d+(?:\.\d+)?)(?:kg|lb)?$/i,
  
  // Single: "Single @9" or "1x1 @9"
  single: /^single\s*@?(\d+(?:\.\d+)?)?$/i,
  
  // AMRAP
  amrap: /^amrap$/i,
  
  // EMOM: "EMOM 6" or "6 min EMOM"
  emom: /emom\s*(\d+)|(\d+)\s*min(?:utes?)?\s*emom/i,
  
  // Tempo: "TEMPO 3:1:0" or "3:1:0"
  tempo: /tempo\s*(\d+:\d+:\d+)|^(\d+:\d+:\d+)$/i,
  
  // Compound: "1x1 3x4" (multiple groups)
  compound: /^((?:\d+x\d+(?:-\d+)?(?:\s+@?\d+(?:\.\d+)?%?)?\s*)+)$/i,
};

/**
 * Parse a single set group (e.g., "3x8" or "3x8-12 @8")
 */
function parseSetGroup(notation: string): SetGroup | null {
  const trimmed = notation.trim();
  
  // Check for AMRAP
  if (PATTERNS.amrap.test(trimmed)) {
    return { sets: 1, repsMin: 0, isAMRAP: true };
  }
  
  // Check for Single
  const singleMatch = trimmed.match(PATTERNS.single);
  if (singleMatch) {
    return {
      sets: 1,
      repsMin: 1,
      rpe: singleMatch[1] ? parseFloat(singleMatch[1]) : undefined,
    };
  }
  
  // Check with percentage first (must have %)
  const percentMatch = trimmed.match(PATTERNS.withPercentage);
  if (percentMatch) {
    const [, sets, repsMin, repsMax, percentage] = percentMatch;
    return {
      sets: parseInt(sets),
      repsMin: parseInt(repsMin),
      repsMax: repsMax ? parseInt(repsMax) : undefined,
      percentage: parseFloat(percentage),
    };
  }
  
  // Check with RPE (@X without %)
  const rpeMatch = trimmed.match(PATTERNS.withRpe);
  if (rpeMatch) {
    const [, sets, repsMin, repsMax, rpe] = rpeMatch;
    const rpeValue = parseFloat(rpe);
    // If value > 10, it's likely a weight
    if (rpeValue <= 10) {
      return {
        sets: parseInt(sets),
        repsMin: parseInt(repsMin),
        repsMax: repsMax ? parseInt(repsMax) : undefined,
        rpe: rpeValue,
      };
    } else {
      // It's a weight
      return {
        sets: parseInt(sets),
        repsMin: parseInt(repsMin),
        repsMax: repsMax ? parseInt(repsMax) : undefined,
        weight: rpeValue,
      };
    }
  }
  
  // Basic pattern without modifiers
  const basicMatch = trimmed.match(PATTERNS.basic);
  if (basicMatch) {
    const [, sets, repsMin, repsMax] = basicMatch;
    return {
      sets: parseInt(sets),
      repsMin: parseInt(repsMin),
      repsMax: repsMax ? parseInt(repsMax) : undefined,
    };
  }
  
  return null;
}

/**
 * Main parser function
 */
export function parseSetNotation(input: string): ParsedNotation {
  if (!input || typeof input !== 'string') {
    return {
      groups: [],
      raw: input || '',
      isValid: false,
      error: 'Empty input',
    };
  }

  const raw = input.trim();
  let workingString = raw;
  let tempo: string | undefined;
  let emomMinutes: number | undefined;

  // Extract EMOM if present
  const emomMatch = workingString.match(PATTERNS.emom);
  if (emomMatch) {
    emomMinutes = parseInt(emomMatch[1] || emomMatch[2]);
    workingString = workingString.replace(PATTERNS.emom, '').trim();
  }

  // Extract Tempo if present
  const tempoMatch = workingString.match(PATTERNS.tempo);
  if (tempoMatch) {
    tempo = tempoMatch[1] || tempoMatch[2];
    workingString = workingString.replace(PATTERNS.tempo, '').trim();
    // Remove "TEMPO" word if present
    workingString = workingString.replace(/tempo\s*/i, '').trim();
  }

  // Try to parse as compound notation (multiple groups separated by space)
  // Split by spaces but keep percentages attached
  const parts = workingString.split(/\s+(?=\d+x)/i).filter(Boolean);
  
  if (parts.length === 0) {
    // Try single group
    const singleGroup = parseSetGroup(workingString);
    if (singleGroup) {
      return {
        groups: [{ ...singleGroup, tempo }],
        emomMinutes,
        tempo,
        raw,
        isValid: true,
      };
    }
    
    return {
      groups: [],
      raw,
      isValid: false,
      error: `Unable to parse: "${raw}"`,
    };
  }

  const groups: SetGroup[] = [];
  
  for (const part of parts) {
    const group = parseSetGroup(part);
    if (group) {
      if (tempo) group.tempo = tempo;
      groups.push(group);
    }
  }

  if (groups.length === 0) {
    return {
      groups: [],
      raw,
      isValid: false,
      error: `Unable to parse: "${raw}"`,
    };
  }

  return {
    groups,
    emomMinutes,
    tempo,
    raw,
    isValid: true,
  };
}

/**
 * Format a parsed notation back to string
 */
export function formatSetNotation(parsed: ParsedNotation): string {
  if (!parsed.isValid || parsed.groups.length === 0) {
    return parsed.raw || '';
  }

  const parts: string[] = [];

  for (const group of parsed.groups) {
    if (group.isAMRAP) {
      parts.push('AMRAP');
      continue;
    }

    let groupStr = `${group.sets}x${group.repsMin}`;
    
    if (group.repsMax && group.repsMax !== group.repsMin) {
      groupStr += `-${group.repsMax}`;
    }
    
    if (group.percentage) {
      groupStr += ` ${group.percentage}%`;
    } else if (group.rpe) {
      groupStr += ` @${group.rpe}`;
    } else if (group.weight) {
      groupStr += ` @${group.weight}`;
    }
    
    parts.push(groupStr);
  }

  let result = parts.join(' ');

  if (parsed.tempo) {
    result += ` TEMPO ${parsed.tempo}`;
  }

  if (parsed.emomMinutes) {
    result += ` EMOM ${parsed.emomMinutes}`;
  }

  return result;
}

/**
 * Convert parsed notation to BuilderExercise fields
 */
export function notationToBuilderFields(parsed: ParsedNotation): {
  sets: number;
  repsMin: number;
  repsMax: number;
  rpeTarget?: number;
  percentageRM?: number;
  notes?: string;
  rawNotation?: string;
} {
  if (!parsed.isValid || parsed.groups.length === 0) {
    return { sets: 0, repsMin: 0, repsMax: 0 };
  }

  // For compound notation, we store the raw string in rawNotation
  // and use the first group for primary fields
  const firstGroup = parsed.groups[0];
  
  let notes = '';
  let rawNotation: string | undefined;
  
  if (parsed.groups.length > 1) {
    // Store full notation in rawNotation for compound sets
    rawNotation = parsed.raw;
  }
  if (parsed.tempo) {
    notes += (notes ? ' | ' : '') + `Tempo: ${parsed.tempo}`;
  }
  if (parsed.emomMinutes) {
    notes += (notes ? ' | ' : '') + `EMOM: ${parsed.emomMinutes} min`;
  }

  return {
    sets: firstGroup.sets,
    repsMin: firstGroup.repsMin,
    repsMax: firstGroup.repsMax || firstGroup.repsMin,
    rpeTarget: firstGroup.rpe,
    percentageRM: firstGroup.percentage,
    notes: notes || undefined,
    rawNotation,
  };
}

/**
 * Convert BuilderExercise fields to display notation
 */
export function builderFieldsToNotation(fields: {
  sets?: number;
  repsMin?: number;
  repsMax?: number;
  rpeTarget?: number;
  notes?: string;
}): string {
  const { sets, repsMin, repsMax, rpeTarget, notes } = fields;
  
  if (!sets || !repsMin) {
    return notes || '';
  }

  let result = `${sets}x${repsMin}`;
  
  if (repsMax && repsMax !== repsMin) {
    result += `-${repsMax}`;
  }
  
  if (rpeTarget) {
    result += ` @${rpeTarget}`;
  }

  return result;
}

/**
 * Validate a notation string
 */
export function isValidNotation(input: string): boolean {
  const parsed = parseSetNotation(input);
  return parsed.isValid;
}

/**
 * Get suggestions for autocomplete based on partial input
 */
export function getNotationSuggestions(input: string): string[] {
  if (!input) {
    return [
      '3x8',
      '3x10-12',
      '4x6 @8',
      '3x5 75%',
      '1x1 3x4',
      'Single @9',
      'AMRAP',
      '3x10 TEMPO 3:1:0',
    ];
  }

  const suggestions: string[] = [];
  const trimmed = input.trim();

  // If starts with number, suggest completions
  if (/^\d+$/.test(trimmed)) {
    const num = trimmed;
    suggestions.push(`${num}x8`, `${num}x10`, `${num}x12`, `${num}x5`);
  }
  
  // If has "x", suggest reps
  if (/^\d+x$/.test(trimmed)) {
    const sets = trimmed.slice(0, -1);
    suggestions.push(`${sets}x8`, `${sets}x10`, `${sets}x12`, `${sets}x5`);
  }
  
  // If has sets and reps, suggest modifiers
  if (/^\d+x\d+$/.test(trimmed)) {
    suggestions.push(
      `${trimmed} @8`,
      `${trimmed} @9`,
      `${trimmed} 70%`,
      `${trimmed} 75%`,
      `${trimmed}-${parseInt(trimmed.split('x')[1]) + 2}`,
    );
  }

  return suggestions.slice(0, 6);
}
