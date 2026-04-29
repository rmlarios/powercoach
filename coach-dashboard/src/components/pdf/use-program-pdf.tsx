'use client';

import { useState, useCallback } from 'react';
import { pdf } from '@react-pdf/renderer';
import { ProgramPDFDocument, type ProgramPDFData } from './program-pdf-document';

// Re-export ProgramPDFData for convenience
export type { ProgramPDFData };

export interface UseProgramPDFOptions {
  /** Whether to show the preview before downloading */
  showPreview?: boolean;
  /** Filename for the downloaded PDF (without extension) */
  filename?: string;
}

export interface UseProgramPDFReturn {
  /** Generate and download the PDF */
  downloadPDF: (data: ProgramPDFData) => Promise<void>;
  /** Generate PDF blob for preview */
  generatePDFBlob: (data: ProgramPDFData) => Promise<Blob>;
  /** Generate PDF URL for preview iframe */
  generatePDFUrl: (data: ProgramPDFData) => Promise<string>;
  /** Loading state */
  isGenerating: boolean;
  /** Error state */
  error: Error | null;
}

/**
 * Hook for generating and downloading program PDFs
 */
export function useProgramPDF(options: UseProgramPDFOptions = {}): UseProgramPDFReturn {
  const { filename = 'programa' } = options;
  const [isGenerating, setIsGenerating] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const generatePDFBlob = useCallback(async (data: ProgramPDFData): Promise<Blob> => {
    setIsGenerating(true);
    setError(null);

    try {
      const blob = await pdf(<ProgramPDFDocument {...data} />).toBlob();
      return blob;
    } catch (err) {
      const error = err instanceof Error ? err : new Error('Failed to generate PDF');
      setError(error);
      throw error;
    } finally {
      setIsGenerating(false);
    }
  }, []);

  const generatePDFUrl = useCallback(async (data: ProgramPDFData): Promise<string> => {
    const blob = await generatePDFBlob(data);
    return URL.createObjectURL(blob);
  }, [generatePDFBlob]);

  const downloadPDF = useCallback(async (data: ProgramPDFData): Promise<void> => {
    setIsGenerating(true);
    setError(null);

    try {
      const blob = await pdf(<ProgramPDFDocument {...data} />).toBlob();
      
      // Create download link
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `${filename}_${data.athleteName || 'programa'}.pdf`;
      
      // Trigger download
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      
      // Cleanup
      URL.revokeObjectURL(url);
    } catch (err) {
      const error = err instanceof Error ? err : new Error('Failed to download PDF');
      setError(error);
      throw error;
    } finally {
      setIsGenerating(false);
    }
  }, [filename]);

  return {
    downloadPDF,
    generatePDFBlob,
    generatePDFUrl,
    isGenerating,
    error,
  };
}
