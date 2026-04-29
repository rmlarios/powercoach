'use client';

import React, { useState, useEffect } from 'react';
import { FileDown, Loader2, Eye, X, Settings2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Switch } from '@/components/ui/switch';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Label } from '@/components/ui/label';
import { useProgramPDF, type ProgramPDFData } from './use-program-pdf';

interface ExportPDFDialogProps {
  /** PDF data to export */
  pdfData: ProgramPDFData;
  /** Trigger button content */
  children?: React.ReactNode;
  /** Called after successful export */
  onExported?: () => void;
  /** Controlled open state */
  open?: boolean;
  /** Callback when open state changes */
  onOpenChange?: (open: boolean) => void;
}

export function ExportPDFDialog({
  pdfData,
  children,
  onExported,
  open: controlledOpen,
  onOpenChange,
}: ExportPDFDialogProps) {
  const [internalOpen, setInternalOpen] = useState(false);
  
  // Support both controlled and uncontrolled modes
  const isControlled = controlledOpen !== undefined;
  const open = isControlled ? controlledOpen : internalOpen;
  const setOpen = (value: boolean) => {
    if (!isControlled) {
      setInternalOpen(value);
    }
    onOpenChange?.(value);
  };
  const [showPreview, setShowPreview] = useState(false);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  
  // Export options
  const [includeWeights, setIncludeWeights] = useState(true);
  const [includeNotes, setIncludeNotes] = useState(true);
  const [weightRoundTo, setWeightRoundTo] = useState<number>(2.5);

  const { downloadPDF, generatePDFUrl, isGenerating, error } = useProgramPDF({
    filename: pdfData.programName.replace(/\s+/g, '_').toLowerCase(),
  });

  // Cleanup preview URL when dialog closes
  useEffect(() => {
    if (!open && previewUrl) {
      URL.revokeObjectURL(previewUrl);
      setPreviewUrl(null);
      setShowPreview(false);
    }
  }, [open, previewUrl]);

  const handlePreview = async () => {
    try {
      const url = await generatePDFUrl({
        ...pdfData,
        includeWeights,
        includeNotes,
        weightRoundTo,
      });
      setPreviewUrl(url);
      setShowPreview(true);
    } catch (err) {
      console.error('Failed to generate preview:', err);
    }
  };

  const handleDownload = async () => {
    try {
      await downloadPDF({
        ...pdfData,
        includeWeights,
        includeNotes,
        weightRoundTo,
      });
      onExported?.();
      setOpen(false);
    } catch (err) {
      console.error('Failed to download PDF:', err);
    }
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      {/* Only render trigger when not externally controlled */}
      {!isControlled && (
        <DialogTrigger asChild>
          {children || (
            <Button variant="outline" size="sm">
              <FileDown className="h-4 w-4 mr-2" />
              Export PDF
            </Button>
          )}
        </DialogTrigger>
      )}
      <DialogContent className={showPreview ? 'max-w-5xl h-[90vh]' : 'max-w-md'}>
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <FileDown className="h-5 w-5" />
            Exportar a PDF
          </DialogTitle>
          <DialogDescription>
            Genera un PDF profesional con el programa de entrenamiento
          </DialogDescription>
        </DialogHeader>

        <div className="flex gap-4">
          {/* Options Panel */}
          <div className={`space-y-4 ${showPreview ? 'w-64 shrink-0' : 'flex-1'}`}>
            {/* Program Info */}
            <div className="space-y-2 pb-4 border-b">
              <h4 className="font-medium text-sm">Información</h4>
              <div className="text-sm text-muted-foreground space-y-1">
                <p><strong>Programa:</strong> {pdfData.programName}</p>
                {pdfData.athleteName && <p><strong>Atleta:</strong> {pdfData.athleteName}</p>}
                <p><strong>Duración:</strong> {pdfData.durationWeeks} semanas</p>
              </div>
            </div>

            {/* Export Options */}
            <div className="space-y-4">
              <h4 className="font-medium text-sm flex items-center gap-2">
                <Settings2 className="h-4 w-4" />
                Opciones
              </h4>

              <div className="flex items-center justify-between">
                <Label htmlFor="includeWeights" className="text-sm cursor-pointer">
                  Incluir pesos calculados
                </Label>
                <Switch
                  id="includeWeights"
                  checked={includeWeights}
                  onCheckedChange={setIncludeWeights}
                />
              </div>

              <div className="flex items-center justify-between">
                <Label htmlFor="includeNotes" className="text-sm cursor-pointer">
                  Incluir notas
                </Label>
                <Switch
                  id="includeNotes"
                  checked={includeNotes}
                  onCheckedChange={setIncludeNotes}
                />
              </div>

              {includeWeights && (
                <div className="space-y-2">
                  <Label htmlFor="weightRoundTo" className="text-sm">
                    Redondeo de pesos
                  </Label>
                  <Select
                    value={weightRoundTo.toString()}
                    onValueChange={(v) => setWeightRoundTo(parseFloat(v))}
                  >
                    <SelectTrigger className="w-full">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="1">1 kg</SelectItem>
                      <SelectItem value="2.5">2.5 kg</SelectItem>
                      <SelectItem value="5">5 kg</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              )}
            </div>

            {/* Error Display */}
            {error && (
              <div className="p-3 text-sm text-red-600 bg-red-50 rounded-md">
                Error: {error.message}
              </div>
            )}

            {/* Action Buttons */}
            <div className="flex flex-col gap-2 pt-4 border-t">
              <Button
                onClick={handlePreview}
                variant="outline"
                disabled={isGenerating}
              >
                {isGenerating && !showPreview ? (
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                ) : (
                  <Eye className="h-4 w-4 mr-2" />
                )}
                {showPreview ? 'Actualizar Vista Previa' : 'Vista Previa'}
              </Button>

              <Button
                onClick={handleDownload}
                disabled={isGenerating}
              >
                {isGenerating ? (
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                ) : (
                  <FileDown className="h-4 w-4 mr-2" />
                )}
                Descargar PDF
              </Button>
            </div>
          </div>

          {/* Preview Panel */}
          {showPreview && (
            <div className="flex-1 relative bg-muted rounded-lg overflow-hidden">
              {previewUrl ? (
                <iframe
                  src={previewUrl}
                  className="w-full h-full border-0"
                  title="PDF Preview"
                />
              ) : (
                <div className="flex items-center justify-center h-full">
                  <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
                </div>
              )}
              <Button
                variant="ghost"
                size="icon"
                className="absolute top-2 right-2"
                onClick={() => {
                  if (previewUrl) {
                    URL.revokeObjectURL(previewUrl);
                  }
                  setPreviewUrl(null);
                  setShowPreview(false);
                }}
              >
                <X className="h-4 w-4" />
              </Button>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
