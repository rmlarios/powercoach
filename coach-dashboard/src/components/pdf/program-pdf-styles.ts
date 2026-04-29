import { StyleSheet } from '@react-pdf/renderer';

// Register fonts (optional - use system fonts for now)
// Font.register({
//   family: 'Inter',
//   src: 'https://fonts.gstatic.com/s/inter/v12/UcCO3FwrK3iLTeHuS_fvQtMwCp50KnMw2boKoduKmMEVuLyfAZ9hiJ-Ek-_EeA.woff2',
// });

/**
 * PDF Styles for Program Export
 * Uses @react-pdf/renderer StyleSheet
 */
export const pdfStyles = StyleSheet.create({
  // Page layout
  page: {
    flexDirection: 'column',
    backgroundColor: '#FFFFFF',
    padding: 30,
    fontFamily: 'Helvetica',
    fontSize: 10,
  },
  
  // Header section
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: 20,
    paddingBottom: 15,
    borderBottomWidth: 2,
    borderBottomColor: '#1e293b',
    borderBottomStyle: 'solid',
  },
  
  headerLeft: {
    flex: 1,
  },
  
  headerRight: {
    alignItems: 'flex-end',
  },
  
  logo: {
    width: 60,
    height: 60,
    marginBottom: 5,
  },
  
  title: {
    fontSize: 22,
    fontWeight: 'bold',
    color: '#1e293b',
    marginBottom: 4,
  },
  
  subtitle: {
    fontSize: 14,
    color: '#64748b',
    marginBottom: 2,
  },
  
  metaText: {
    fontSize: 10,
    color: '#64748b',
  },
  
  // Day section
  daySection: {
    marginTop: 15,
    marginBottom: 10,
  },
  
  dayHeader: {
    backgroundColor: '#1e293b',
    padding: 8,
    marginBottom: 1,
  },
  
  dayTitle: {
    fontSize: 12,
    fontWeight: 'bold',
    color: '#FFFFFF',
    textTransform: 'uppercase',
  },
  
  dayFocus: {
    fontSize: 9,
    color: '#94a3b8',
    marginTop: 2,
  },
  
  // Exercise table
  table: {
    width: '100%',
    marginBottom: 10,
  },
  
  tableHeader: {
    flexDirection: 'row',
    backgroundColor: '#f1f5f9',
    borderBottomWidth: 1,
    borderBottomColor: '#e2e8f0',
    borderBottomStyle: 'solid',
  },
  
  tableRow: {
    flexDirection: 'row',
    borderBottomWidth: 1,
    borderBottomColor: '#e2e8f0',
    borderBottomStyle: 'solid',
    minHeight: 28,
  },
  
  tableRowAlt: {
    backgroundColor: '#f8fafc',
  },
  
  // Table columns
  colExercise: {
    flex: 2,
    padding: 6,
    justifyContent: 'center',
  },
  
  colWeek: {
    flex: 1,
    padding: 6,
    justifyContent: 'center',
    alignItems: 'center',
    borderLeftWidth: 1,
    borderLeftColor: '#e2e8f0',
    borderLeftStyle: 'solid',
  },
  
  // Text styles
  exerciseName: {
    fontSize: 10,
    fontWeight: 'bold',
    color: '#1e293b',
  },
  
  exerciseType: {
    fontSize: 8,
    color: '#64748b',
    marginTop: 2,
  },
  
  headerCell: {
    fontSize: 9,
    fontWeight: 'bold',
    color: '#475569',
    textAlign: 'center',
  },
  
  prescription: {
    fontSize: 10,
    color: '#1e293b',
    textAlign: 'center',
  },
  
  intensity: {
    fontSize: 8,
    color: '#64748b',
    textAlign: 'center',
    marginTop: 2,
  },
  
  weight: {
    fontSize: 8,
    color: '#059669',
    textAlign: 'center',
    marginTop: 1,
    fontWeight: 'bold',
  },
  
  // Footer
  footer: {
    position: 'absolute',
    bottom: 20,
    left: 30,
    right: 30,
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingTop: 10,
    borderTopWidth: 1,
    borderTopColor: '#e2e8f0',
    borderTopStyle: 'solid',
  },
  
  footerText: {
    fontSize: 8,
    color: '#94a3b8',
  },
  
  pageNumber: {
    fontSize: 8,
    color: '#64748b',
  },
  
  // Notes section
  notesSection: {
    marginTop: 15,
    padding: 10,
    backgroundColor: '#f8fafc',
    borderRadius: 4,
  },
  
  notesTitle: {
    fontSize: 10,
    fontWeight: 'bold',
    color: '#475569',
    marginBottom: 5,
  },
  
  notesText: {
    fontSize: 9,
    color: '#64748b',
    lineHeight: 1.4,
  },
  
  // Badge styles
  badge: {
    paddingHorizontal: 4,
    paddingVertical: 2,
    borderRadius: 4,
    marginLeft: 4,
  },
  
  badgeEmom: {
    backgroundColor: '#dbeafe',
  },
  
  badgeTempo: {
    backgroundColor: '#f3e8ff',
  },
  
  badgeSuperset: {
    backgroundColor: '#fed7aa',
  },
  
  badgeText: {
    fontSize: 7,
    fontWeight: 'bold',
  },
  
  // Week header with number
  weekNumber: {
    fontSize: 8,
    color: '#94a3b8',
  },
});

/**
 * Color map for exercise types
 */
export const exerciseTypeColors = {
  emom: { bg: '#dbeafe', text: '#1d4ed8' },
  tempo: { bg: '#f3e8ff', text: '#7c3aed' },
  superset: { bg: '#fed7aa', text: '#c2410c' },
  circuit: { bg: '#d1fae5', text: '#059669' },
  amrap: { bg: '#fecaca', text: '#dc2626' },
  standard: { bg: '#f1f5f9', text: '#475569' },
};
