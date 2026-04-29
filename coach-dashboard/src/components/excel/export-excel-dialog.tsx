'use client';

import React, { useState } from 'react';
import { FileSpreadsheet, Loader2, Download } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Switch } from '@/components/ui/switch';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Label } from '@/components/ui/label';
import { downloadExcel, type ProgramExcelData } from './program-excel-generator';

interface ExportExcelDialogProps {
  /** Excel data to export */
  excelData: ProgramExcelData;
  /** Called after successful export */
  onExported?: () => void;
  /** Controlled open state */
  open?: boolean;
  /** Callback when open state changes */
  onOpenChange?: (open: boolean) => void;
}

export function ExportExcelDialog({
  excelData,
  onExported,
  open: controlledOpen,
  onOpenChange,
}: ExportExcelDialogProps) {
  const [internalOpen, setInternalOpen] = useState(false);
  const [isGenerating, setIsGenerating] = useState(false);
  
  // Support both controlled and uncontrolled modes
  const isControlled = controlledOpen !== undefined;
  const open = isControlled ? controlledOpen : internalOpen;
  const setOpen = (value: boolean) => {
    if (!isControlled) {
      setInternalOpen(value);
    }
    onOpenChange?.(value);
  };
  
  // Export options
  const [includeWeights, setIncludeWeights] = useState(true);
  const [weightRoundTo, setWeightRoundTo] = useState<number>(2.5);

  const handleDownload = async () => {
    setIsGenerating(true);
    
    try {
      // Small delay to show loading state
      await new Promise(resolve => setTimeout(resolve, 100));
      
      downloadExcel({
        ...excelData,
        includeWeights,
        weightRoundTo,
      });
      
      onExported?.();
      setOpen(false);
    } catch (err) {
      console.error('Failed to generate Excel:', err);
    } finally {
      setIsGenerating(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <FileSpreadsheet className="h-5 w-5 text-green-600" />
            Exportar a Excel
          </DialogTitle>
          <DialogDescription>
            Genera un archivo Excel con el programa de entrenamiento
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6 py-4">
          {/* Program Info */}
          <div className="bg-muted/50 rounded-lg p-4 space-y-2">
            <h4 className="font-medium text-sm">Información del programa</h4>
            <div className="text-sm text-muted-foreground space-y-1">
              <p><strong>Nombre:</strong> {excelData.programName}</p>
              {excelData.athleteName && (
                <p><strong>Atleta:</strong> {excelData.athleteName}</p>
              )}
              <p><strong>Duración:</strong> {excelData.durationWeeks} semanas</p>
            </div>
          </div>

          {/* Export Options */}
          <div className="space-y-4">
            <h4 className="font-medium text-sm">Opciones de exportación</h4>

            <div className="flex items-center justify-between">
              <Label htmlFor="includeWeightsExcel" className="text-sm cursor-pointer">
                Incluir pesos calculados
              </Label>
              <Switch
                id="includeWeightsExcel"
                checked={includeWeights}
                onCheckedChange={setIncludeWeights}
              />
            </div>

            {includeWeights && (
              <div className="space-y-2">
                <Label htmlFor="weightRoundToExcel" className="text-sm">
                  Redondear pesos a
                </Label>
                <Select
                  value={weightRoundTo.toString()}
                  onValueChange={(v) => setWeightRoundTo(parseFloat(v))}
                >
                  <SelectTrigger id="weightRoundToExcel">
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

          {/* Structure Info */}
          <div className="bg-green-50 dark:bg-green-950/20 rounded-lg p-4 text-sm">
            <p className="text-green-800 dark:text-green-200">
              <strong>Estructura del Excel:</strong>
            </p>
            <ul className="mt-2 space-y-1 text-green-700 dark:text-green-300">
              <li>• Hoja &quot;Overview&quot; con info general</li>
              <li>• Una hoja por cada día de entrenamiento</li>
              <li>• Ejercicios como filas, semanas como columnas</li>
              <li>• Columnas y filas congeladas</li>
            </ul>
          </div>
        </div>

        {/* Actions */}
        <div className="flex justify-end gap-2">
          <Button variant="outline" onClick={() => setOpen(false)}>
            Cancelar
          </Button>
          <Button onClick={handleDownload} disabled={isGenerating}>
            {isGenerating ? (
              <>
                <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                Generando...
              </>
            ) : (
              <>
                <Download className="h-4 w-4 mr-2" />
                Descargar Excel
              </>
            )}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
