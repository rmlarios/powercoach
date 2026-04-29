'use client';

import * as XLSX from 'xlsx';
import type { BuilderWeek } from '@/types/builder';
import type { MaxLift } from '@/types/athlete';
import {
  transformToAllDays,
  createOverviewData,
  createDaySheetData,
} from './excel-utils';

// ============================================
// Types
// ============================================
export interface ProgramExcelData {
  programName: string;
  description?: string;
  athleteName?: string;
  coachName?: string;
  startDate?: string;
  durationWeeks: number;
  weeks: BuilderWeek[];
  athleteMaxLifts?: MaxLift[];
  includeWeights?: boolean;
  weightRoundTo?: number;
}

// ============================================
// Excel Generation
// ============================================
export function generateProgramExcel(data: ProgramExcelData): XLSX.WorkBook {
  const {
    durationWeeks,
    weeks,
    athleteMaxLifts,
    includeWeights = true,
    weightRoundTo = 2.5,
  } = data;

  const workbook = XLSX.utils.book_new();

  // Create Overview/Summary sheet
  const overviewRows = createOverviewData(data);
  const overviewSheet = XLSX.utils.aoa_to_sheet(overviewRows);
  
  // Set column widths for overview
  overviewSheet['!cols'] = [
    { wch: 20 }, // Label column
    { wch: 40 }, // Value column
  ];
  
  XLSX.utils.book_append_sheet(workbook, overviewSheet, 'Overview');

  // Transform data to day-centric structure
  const allDays = transformToAllDays(weeks);
  const weekNumbers = Array.from({ length: durationWeeks }, (_, i) => i + 1);

  // Create one sheet per day
  allDays.forEach((dayData) => {
    const sheetData = createDaySheetData(
      dayData,
      weekNumbers,
      athleteMaxLifts,
      includeWeights,
      weightRoundTo
    );
    
    const sheet = XLSX.utils.aoa_to_sheet(sheetData);
    
    // Set column widths
    const colWidths = [
      { wch: 25 }, // Exercise name
      ...weekNumbers.map(() => ({ wch: 15 })), // Week columns
    ];
    sheet['!cols'] = colWidths;
    
    // Freeze first row and first column
    sheet['!freeze'] = { xSplit: 1, ySplit: 1 };
    
    // Sheet name (max 31 chars for Excel)
    const sheetName = dayData.name.substring(0, 31);
    XLSX.utils.book_append_sheet(workbook, sheet, sheetName);
  });

  return workbook;
}

// ============================================
// Export Functions
// ============================================
export function downloadExcel(data: ProgramExcelData, filename?: string): void {
  const workbook = generateProgramExcel(data);
  const finalFilename = filename || `${data.programName.replace(/\s+/g, '_').toLowerCase()}.xlsx`;
  
  XLSX.writeFile(workbook, finalFilename);
}

export function generateExcelBlob(data: ProgramExcelData): Blob {
  const workbook = generateProgramExcel(data);
  const wbout = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
  return new Blob([wbout], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
}
