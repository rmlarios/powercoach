'use client';

import React from 'react';
import {
  Document,
  Page,
  Text,
  View,
  Image,
} from '@react-pdf/renderer';
import { pdfStyles as styles } from './program-pdf-styles';
import type { BuilderWeek } from '@/types/builder';
import type { MaxLift } from '@/types/athlete';
import {
  transformToAllDays,
  getExercise1RM,
  formatWeight,
  type DayExerciseForPDF,
} from './pdf-utils';
import { calculateWorkingWeight } from '@/utils/weight-calculator';

// ============================================
// Types
// ============================================
export interface ProgramPDFData {
  programName: string;
  description?: string;
  athleteName?: string;
  coachName?: string;
  startDate?: string;
  durationWeeks: number;
  weeks: BuilderWeek[];
  athleteMaxLifts?: MaxLift[];
  logoUrl?: string;
  includeWeights?: boolean;
  includeNotes?: boolean;
  weightRoundTo?: number;
}

// transformToAllDays, getExercise1RM, formatWeight imported from './pdf-utils'

// ============================================
// PDF Components
// ============================================
interface HeaderProps {
  programName: string;
  description?: string;
  athleteName?: string;
  coachName?: string;
  startDate?: string;
  durationWeeks: number;
  logoUrl?: string;
}

function Header({
  programName,
  description,
  athleteName,
  coachName,
  startDate,
  durationWeeks,
  logoUrl,
}: HeaderProps) {
  return (
    <View style={styles.header}>
      <View style={styles.headerLeft}>
        <Text style={styles.title}>{programName}</Text>
        {description && <Text style={styles.subtitle}>{description}</Text>}
        <View style={{ marginTop: 8 }}>
          {athleteName && (
            <Text style={styles.metaText}>Atleta: {athleteName}</Text>
          )}
          {coachName && (
            <Text style={styles.metaText}>Coach: {coachName}</Text>
          )}
          <Text style={styles.metaText}>Duración: {durationWeeks} semanas</Text>
          {startDate && (
            <Text style={styles.metaText}>Inicio: {startDate}</Text>
          )}
        </View>
      </View>
      <View style={styles.headerRight}>
        {logoUrl && (
          // eslint-disable-next-line jsx-a11y/alt-text
          <Image src={logoUrl} style={styles.logo} />
        )}
        <Text style={styles.metaText}>
          Generado: {new Date().toLocaleDateString('es-ES')}
        </Text>
      </View>
    </View>
  );
}

interface ExerciseTableProps {
  exercises: DayExerciseForPDF[];
  totalWeeks: number;
  athleteMaxLifts?: MaxLift[];
  includeWeights?: boolean;
  weightRoundTo?: number;
}

function ExerciseTable({
  exercises,
  totalWeeks,
  athleteMaxLifts,
  includeWeights = true,
  weightRoundTo = 2.5,
}: ExerciseTableProps) {
  const weekNumbers = Array.from({ length: totalWeeks }, (_, i) => i + 1);

  return (
    <View style={styles.table}>
      {/* Table Header */}
      <View style={styles.tableHeader}>
        <View style={styles.colExercise}>
          <Text style={styles.headerCell}>Ejercicio</Text>
        </View>
        {weekNumbers.map((weekNum) => (
          <View key={weekNum} style={styles.colWeek}>
            <Text style={styles.headerCell}>S{weekNum}</Text>
          </View>
        ))}
      </View>

      {/* Table Rows */}
      {exercises.map((exercise, index) => {
        const oneRM = getExercise1RM(exercise.exerciseId, athleteMaxLifts);
        const rowStyle = index % 2 === 1 
          ? [styles.tableRow, styles.tableRowAlt]
          : [styles.tableRow];
        
        return (
          <View
            key={exercise.exerciseId}
            style={rowStyle}
          >
            {/* Exercise Name Column */}
            <View style={styles.colExercise}>
              <Text style={styles.exerciseName}>{exercise.exerciseName}</Text>
              {exercise.exerciseType && exercise.exerciseType !== 'standard' && (
                <Text style={styles.exerciseType}>
                  {exercise.exerciseType.toUpperCase()}
                </Text>
              )}
            </View>

            {/* Week Columns */}
            {weekNumbers.map((weekNum) => {
              const prescription = exercise.weekPrescriptions.get(weekNum);
              
              if (!prescription) {
                return (
                  <View key={weekNum} style={styles.colWeek}>
                    <Text style={[styles.prescription, { color: '#94a3b8' }]}>—</Text>
                  </View>
                );
              }

              // Calculate weight if percentage and 1RM are available
              let calculatedWeight: number | null = null;
              if (includeWeights && prescription.percentageRM && oneRM) {
                calculatedWeight = calculateWorkingWeight(
                  oneRM,
                  prescription.percentageRM,
                  weightRoundTo
                );
              }
              const displayWeight = prescription.weight ?? calculatedWeight;

              return (
                <View key={weekNum} style={styles.colWeek}>
                  <Text style={styles.prescription}>{prescription.reps}</Text>
                  {prescription.percentageRM && (
                    <Text style={styles.intensity}>{prescription.percentageRM}%</Text>
                  )}
                  {prescription.rpeTarget && !prescription.percentageRM && (
                    <Text style={styles.intensity}>@{prescription.rpeTarget}</Text>
                  )}
                  {includeWeights && displayWeight && (
                    <Text style={styles.weight}>{formatWeight(displayWeight)}</Text>
                  )}
                </View>
              );
            })}
          </View>
        );
      })}
    </View>
  );
}

interface DaySectionProps {
  dayNumber: number;
  dayName: string;
  exercises: DayExerciseForPDF[];
  totalWeeks: number;
  athleteMaxLifts?: MaxLift[];
  includeWeights?: boolean;
  weightRoundTo?: number;
}

function DaySection({
  dayNumber,
  dayName,
  exercises,
  totalWeeks,
  athleteMaxLifts,
  includeWeights,
  weightRoundTo,
}: DaySectionProps) {
  return (
    <View style={styles.daySection} wrap={false}>
      <View style={styles.dayHeader}>
        <Text style={styles.dayTitle}>
          DÍA {dayNumber} - {dayName.toUpperCase()}
        </Text>
      </View>
      <ExerciseTable
        exercises={exercises}
        totalWeeks={totalWeeks}
        athleteMaxLifts={athleteMaxLifts}
        includeWeights={includeWeights}
        weightRoundTo={weightRoundTo}
      />
    </View>
  );
}

interface FooterProps {
  totalPages?: number;
  coachName?: string;
}

function Footer({ coachName }: FooterProps) {
  return (
    <View style={styles.footer} fixed>
      <Text style={styles.footerText}>
        {coachName ? `Coach: ${coachName}` : 'PowerCoach'}
      </Text>
      <Text
        style={styles.pageNumber}
        render={({ pageNumber, totalPages }) => `${pageNumber} / ${totalPages}`}
      />
    </View>
  );
}

// ============================================
// Main PDF Document
// ============================================
export function ProgramPDFDocument({
  programName,
  description,
  athleteName,
  coachName,
  startDate,
  durationWeeks,
  weeks,
  athleteMaxLifts,
  logoUrl,
  includeWeights = true,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  includeNotes = true,
  weightRoundTo = 2.5,
}: ProgramPDFData) {
  // Transform weeks to day-centric structure
  const allDays = transformToAllDays(weeks);
  const sortedDays = Array.from(allDays.entries()).sort(([a], [b]) => a - b);

  return (
    <Document
      title={`${programName} - ${athleteName || 'Programa'}`}
      author={coachName || 'PowerCoach'}
      subject="Training Program"
      creator="PowerCoach"
    >
      <Page size="A4" style={styles.page} wrap>
        {/* Header */}
        <Header
          programName={programName}
          description={description}
          athleteName={athleteName}
          coachName={coachName}
          startDate={startDate}
          durationWeeks={durationWeeks}
          logoUrl={logoUrl}
        />

        {/* Day Sections */}
        {sortedDays.map(([dayNumber, dayData]) => (
          <DaySection
            key={dayNumber}
            dayNumber={dayNumber}
            dayName={dayData.name}
            exercises={dayData.exercises}
            totalWeeks={durationWeeks}
            athleteMaxLifts={athleteMaxLifts}
            includeWeights={includeWeights}
            weightRoundTo={weightRoundTo}
          />
        ))}

        {/* Footer */}
        <Footer coachName={coachName} />
      </Page>
    </Document>
  );
}
