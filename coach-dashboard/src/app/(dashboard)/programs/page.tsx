'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { PageHeader, DataTable, Column, Pagination } from '@/components/common';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
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
import { Plus, Search, Copy, MoreHorizontal, Users } from 'lucide-react';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { usePrograms, useCreateProgram, useCloneProgram, useBulkAssignProgram } from '@/hooks';
import { useAthletes } from '@/hooks';
import { useCoach, useAuth } from '@/providers';
import { ProgramTemplateListItem } from '@/types';
import { format } from 'date-fns';
import { toast } from 'sonner';
import { BulkAssignDialog } from '@/components/programs/bulk-assign-dialog';

export default function ProgramsPage() {
  const router = useRouter();
  const { user } = useAuth();
  const { coach } = useCoach();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [newProgram, setNewProgram] = useState({
    name: '',
    description: '',
    durationWeeks: 4,
    daysPerWeek: undefined as number | undefined,
    preset: 'custom' as 'custom' | 'pl3' | 'ul4' | 'ppl6',
  });

  // Use coachId from user JWT first, then coach provider, then seeded fallback for Admin
  const coachId = user?.coachId ?? coach?.id ?? '';

  const { data, isLoading } = usePrograms({
    coachId,
    searchTerm: search || undefined,
    pageNumber: page,
    pageSize: 10,
  });

  const createProgram = useCreateProgram();
  const cloneProgram = useCloneProgram();
  const bulkAssign = useBulkAssignProgram();

  const [bulkAssignTarget, setBulkAssignTarget] = useState<{ id: string; name: string } | null>(null);

  const { data: athletesData } = useAthletes({ coachId, pageSize: 200 });

  const handleCreate = async () => {
    if (!newProgram.name.trim()) return;
    try {
      const id = await createProgram.mutateAsync({
        coachId,
        name: newProgram.name.trim(),
        description: newProgram.description.trim() || undefined,
        durationWeeks: newProgram.durationWeeks,
        daysPerWeek: newProgram.daysPerWeek,
      });
      // Store preset for builder to apply on first load
      if (newProgram.preset !== 'custom') {
        localStorage.setItem(`program-preset-${id}`, newProgram.preset);
      }
      setDialogOpen(false);
      setNewProgram({
        name: '',
        description: '',
        durationWeeks: 4,
        daysPerWeek: undefined,
        preset: 'custom',
      });
      router.push(`/programs/${id}/builder`);
    } catch {
      // error handled by react-query
    }
  };

  const columns: Column<ProgramTemplateListItem>[] = [
    {
      key: 'name',
      header: 'Program Name',
      cell: (row) => (
        <div>
          <p className="font-medium text-slate-900">{row.name}</p>
          {row.description && (
            <p className="text-sm text-slate-500 truncate max-w-xs">
              {row.description}
            </p>
          )}
        </div>
      ),
    },
    {
      key: 'duration',
      header: 'Duration',
      cell: (row) => (
        <span className="text-slate-600">{row.durationWeeks} weeks</span>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      cell: (row) => (
        <Badge variant={row.isActive ? 'default' : 'secondary'}>
          {row.isActive ? 'Active' : 'Inactive'}
        </Badge>
      ),
    },
    {
      key: 'createdAt',
      header: 'Created',
      cell: (row) => (
        <span className="text-slate-500 text-sm">
          {format(new Date(row.createdAt), 'MMM d, yyyy')}
        </span>
      ),
    },
    {
      key: 'actions',
      header: '',
      cell: (row) => (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8"
              onClick={(e) => e.stopPropagation()}
            >
              <MoreHorizontal className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem
              onClick={(e) => {
                e.stopPropagation();
                cloneProgram.mutate({ id: row.id, coachId });
              }}
              disabled={cloneProgram.isPending}
            >
              <Copy className="h-4 w-4 mr-2" />
              Duplicar
            </DropdownMenuItem>
            <DropdownMenuItem
              onClick={(e) => {
                e.stopPropagation();
                setBulkAssignTarget({ id: row.id, name: row.name });
              }}
            >
              <Users className="h-4 w-4 mr-2" />
              Asignar a atletas
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      ),
    },
  ];

  return (
    <div>
      <PageHeader
        title="Training Programs"
        description="Create and manage training program templates"
        actions={
          <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
            <DialogTrigger asChild>
              <Button>
                <Plus className="w-4 h-4 mr-2" />
                New Program
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Create New Program</DialogTitle>
                <DialogDescription>
                  Set up a new training program template. You can add weeks, days and exercises after creation.
                </DialogDescription>
              </DialogHeader>
              <div className="space-y-4 py-4">
                <div className="space-y-2">
                  <Label htmlFor="program-name">Program Name *</Label>
                  <Input
                    id="program-name"
                    placeholder="e.g. Hypertrophy 12-Week Block"
                    value={newProgram.name}
                    onChange={(e) => setNewProgram(prev => ({ ...prev, name: e.target.value }))}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="program-desc">Description</Label>
                  <Input
                    id="program-desc"
                    placeholder="Brief description of the program"
                    value={newProgram.description}
                    onChange={(e) => setNewProgram(prev => ({ ...prev, description: e.target.value }))}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="program-weeks">Duration (weeks) *</Label>
                  <Input
                    id="program-weeks"
                    type="number"
                    min={1}
                    max={52}
                    value={newProgram.durationWeeks}
                    onChange={(e) => setNewProgram(prev => ({ ...prev, durationWeeks: parseInt(e.target.value) || 1 }))}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="program-days">Días por semana</Label>
                  <Input
                    id="program-days"
                    type="number"
                    min={1}
                    max={7}
                    placeholder="Dejar vacío para agregar manualmente"
                    value={newProgram.daysPerWeek ?? ''}
                    onChange={(e) => {
                      const val = e.target.value ? parseInt(e.target.value) : undefined;
                      setNewProgram(prev => ({ ...prev, daysPerWeek: val && val >= 1 && val <= 7 ? val : undefined }));
                    }}
                  />
                  <p className="text-xs text-muted-foreground">
                    Se auto-generarán los días vacíos en cada semana.
                  </p>
                </div>
                <div className="space-y-2">
                  <Label>Plantilla de estructura</Label>
                  <Select
                    value={newProgram.preset}
                    onValueChange={(value: 'custom' | 'pl3' | 'ul4' | 'ppl6') => {
                      const presetDays: Record<string, number | undefined> = {
                        custom: newProgram.daysPerWeek,
                        pl3: 3,
                        ul4: 4,
                        ppl6: 6,
                      };
                      setNewProgram(prev => ({
                        ...prev,
                        preset: value,
                        daysPerWeek: presetDays[value] ?? prev.daysPerWeek,
                      }));
                    }}
                  >
                    <SelectTrigger className="w-full">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="custom">Personalizado</SelectItem>
                      <SelectItem value="pl3">Powerlifting 3 días (Squat / Bench / Deadlift)</SelectItem>
                      <SelectItem value="ul4">Upper/Lower 4 días</SelectItem>
                      <SelectItem value="ppl6">Push/Pull/Legs 6 días</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
              <DialogFooter>
                <Button variant="outline" onClick={() => setDialogOpen(false)}>Cancel</Button>
                <Button
                  onClick={handleCreate}
                  disabled={!newProgram.name.trim() || createProgram.isPending}
                >
                  {createProgram.isPending ? 'Creating...' : 'Create Program'}
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        }
      />

      {/* Search */}
      <div className="flex gap-4 mb-6">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
          <Input
            type="search"
            placeholder="Search programs..."
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
            className="pl-10"
          />
        </div>
      </div>

      {/* Table */}
      <DataTable
        columns={columns}
        data={data?.items || []}
        isLoading={isLoading}
        emptyMessage="No training programs found. Create your first program!"
        rowKey={(row) => row.id}
        onRowClick={(row) => router.push(`/programs/${row.id}/builder`)}
      />

      {/* Pagination */}
      {data && (
        <Pagination
          currentPage={data.pageNumber}
          totalPages={data.totalPages}
          onPageChange={setPage}
        />
      )}

      {/* Bulk Assign Dialog */}
      {bulkAssignTarget && (
        <BulkAssignDialog
          programId={bulkAssignTarget.id}
          programName={bulkAssignTarget.name}
          athletes={athletesData?.items?.map((a) => ({ id: a.id, fullName: a.fullName })) ?? []}
          isOpen={!!bulkAssignTarget}
          onOpenChange={(open) => { if (!open) setBulkAssignTarget(null); }}
          isPending={bulkAssign.isPending}
          onConfirm={(athleteIds, startDate) => {
            bulkAssign.mutate(
              { programId: bulkAssignTarget.id, athleteIds, startDate, coachId },
              {
                onSuccess: (ids) => {
                  toast.success(`Programa asignado a ${ids.length} atleta${ids.length !== 1 ? 's' : ''}.`);
                  setBulkAssignTarget(null);
                },
                onError: () => {
                  toast.error('Error al asignar el programa. Intenta de nuevo.');
                },
              }
            );
          }}
        />
      )}
    </div>
  );
}
