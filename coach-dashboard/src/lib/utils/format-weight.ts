/**
 * Format a weight value with unit
 */
export function formatWeight(
  value: number,
  unit: 'kg' | 'lb' = 'kg',
  decimals: number = 1
): string {
  return `${value.toFixed(decimals)} ${unit}`;
}

/**
 * Convert kilograms to pounds
 */
export function kgToLb(kg: number): number {
  return kg * 2.20462;
}

/**
 * Convert pounds to kilograms
 */
export function lbToKg(lb: number): number {
  return lb / 2.20462;
}

/**
 * Format a weight value, optionally converting between units
 */
export function formatWeightWithConversion(
  value: number,
  sourceUnit: 'kg' | 'lb',
  targetUnit: 'kg' | 'lb',
  decimals: number = 1
): string {
  let convertedValue = value;
  
  if (sourceUnit !== targetUnit) {
    convertedValue = sourceUnit === 'kg' ? kgToLb(value) : lbToKg(value);
  }
  
  return formatWeight(convertedValue, targetUnit, decimals);
}

/**
 * Format height in centimeters to feet and inches
 */
export function formatHeightToFeetInches(cm: number): string {
  const totalInches = cm / 2.54;
  const feet = Math.floor(totalInches / 12);
  const inches = Math.round(totalInches % 12);
  return `${feet}'${inches}"`;
}

/**
 * Format height in centimeters
 */
export function formatHeight(cm: number): string {
  return `${cm} cm`;
}
