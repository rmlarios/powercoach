'use client';

import { useState } from 'react';
import { Check } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { ScrollArea } from '@/components/ui/scroll-area';
import { cn } from '@/utils/cn';

interface Athlete {
  id: string;
  fullName: string;
}

interface BulkAssignDialogProps {
  programId: string;
  programName: string;
  athletes: Athlete[];
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: (athleteIds: string[], startDate: string) => void;
  isPending?: boolean;
}

export function BulkAssignDialog({
  programName,
  athletes,
  isOpen,
  onOpenChange,
  onConfirm,
  isPending,
}: BulkAssignDialogProps) {
  const [search, setSearch] = useState('');
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [startDate, setStartDate] = useState(() => {
    return new Date().toISOString().split('T')[0];
  });

  const filtered = athletes.filter((a) =>
    a.fullName.toLowerCase().includes(search.toLowerCase())
  );

  const toggleAthlete = (id: string) => {
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  const handleConfirm = () => {
    if (selected.size === 0 || !startDate) return;
    onConfirm([...selected], startDate);
  };

  const handleOpenChange = (open: boolean) => {
    if (!open) {
      setSearch('');
      setSelected(new Set());
    }
    onOpenChange(open);
  };

  return (
    <Dialog open={isOpen} onOpenChange={handleOpenChange}>
      <DialogContent className="sm:max-w-[480px]">
        <DialogHeader>
          <DialogTitle>Asignar programa a atletas</DialogTitle>
          <DialogDescription>
            Selecciona uno o más atletas para asignarles{' '}
            <span className="font-medium text-foreground">{programName}</span>.
            Los atletas con programa activo serán omitidos automáticamente.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          {/* Search */}
          <Input
            placeholder="Buscar atleta..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />

          {/* Athlete list */}
          <ScrollArea className="h-52 border rounded-md">
            <div className="p-2 space-y-1">
              {filtered.length === 0 ? (
                <p className="text-sm text-muted-foreground text-center py-6">
                  No se encontraron atletas.
                </p>
              ) : (
                filtered.map((athlete) => {
                  const isSelected = selected.has(athlete.id);
                  return (
                    <button
                      key={athlete.id}
                      onClick={() => toggleAthlete(athlete.id)}
                      className={cn(
                        'w-full flex items-center justify-between rounded-md px-3 py-2 text-sm transition-colors text-left',
                        isSelected
                          ? 'bg-primary/10 text-primary'
                          : 'hover:bg-muted'
                      )}
                    >
                      <span>{athlete.fullName}</span>
                      {isSelected && <Check className="h-4 w-4" />}
                    </button>
                  );
                })
              )}
            </div>
          </ScrollArea>

          {selected.size > 0 && (
            <div className="flex items-center gap-2 flex-wrap">
              <span className="text-xs text-muted-foreground">Seleccionados:</span>
              <Badge variant="secondary">{selected.size} atleta{selected.size !== 1 ? 's' : ''}</Badge>
            </div>
          )}

          {/* Start date */}
          <div className="grid gap-1.5">
            <Label htmlFor="bulk-start-date">Fecha de inicio</Label>
            <Input
              id="bulk-start-date"
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => handleOpenChange(false)}>
            Cancelar
          </Button>
          <Button
            onClick={handleConfirm}
            disabled={selected.size === 0 || !startDate || isPending}
          >
            {isPending
              ? 'Asignando...'
              : `Asignar a ${selected.size > 0 ? selected.size : ''} atleta${selected.size !== 1 ? 's' : ''}`}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
