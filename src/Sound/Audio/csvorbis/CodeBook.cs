/* csvorbis
 * Copyright (C) 2000 ymnk, JCraft,Inc.
 *  
 * Written by: 2000 ymnk<ymnk@jcraft.com>
 * Ported to C# from JOrbis by: Mark Crichton <crichton@gimp.org> 
 *   
 * Thanks go to the JOrbis team, for licencing the code under the
 * LGPL, making my job a lot easier.
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Library General Public License
 * as published by the Free Software Foundation; either version 2 of
 * the License, or (at your option) any later version.
   
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Library General Public License for more details.
 * 
 * You should have received a copy of the GNU Library General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 */


using System;
using System.Runtime.CompilerServices;
using csogg;

namespace csvorbis 
{
	class CodeBook
	{
		internal int dim;            // codebook dimensions (elements per vector)
		internal int entries;        // codebook entries
		internal StaticCodeBook c=new StaticCodeBook();

		internal float[] valuelist; // list of dim*entries actual entry values
		internal int[] codelist;     // list of bitstream codewords for each entry
		internal DecodeAux decode_tree;

		// returns the number of bits
		internal int encode(int a, csBuffer b)
		{
			b.write(codelist[a], c.lengthlist[a]);
			return(c.lengthlist[a]);
		}

		// One the encode side, our vector writers are each designed for a
		// specific purpose, and the encoder is not flexible without modification:
		// 
		// The LSP vector coder uses a single stage nearest-match with no
		// interleave, so no step and no error return.  This is specced by floor0
		// and doesn't change.
		// 
		// Residue0 encoding interleaves, uses multiple stages, and each stage
		// peels of a specific amount of resolution from a lattice (thus we want
		// to match by threshhold, not nearest match).  Residue doesn't *have* to
		// be encoded that way, but to change it, one will need to add more
		// infrastructure on the encode side (decode side is specced and simpler)

		// floor0 LSP (single stage, non interleaved, nearest match)
		// returns entry number and *modifies a* to the quantization value
		internal int errorv(float[] a)
		{
			int bestt=best(a,1);
			for(int k=0;k<dim;k++)
			{
				a[k]=valuelist[bestt*dim+k];
			}
			return(bestt);
		}

		// returns the number of bits and *modifies a* to the quantization value
		internal int encodev(int best, float[] a, csBuffer b)
		{
			for(int k=0;k<dim;k++)
			{
				a[k]=valuelist[best*dim+k];
			}
			return(encode(best,b));
		}

		// res0 (multistage, interleave, lattice)
		// returns the number of bits and *modifies a* to the remainder value
		internal int encodevs(float[] a, csBuffer b, int step,int addmul)
		{
			int best=besterror(a,step,addmul);
			return(encode(best,b));
		}

		internal int[] t=new int[15];  // decodevs_add is synchronized for re-using t.
	
		[MethodImpl(MethodImplOptions.Synchronized)]
		internal int decodevs_add(float[]a, int offset, csBuffer b, int n)
		{
			int step=n/dim;
			int entry;
			int i,j,o;

			if(t.Length<step)
			{
				t=new int[step];
			}

			for(i = 0; i < step; i++)
			{
				entry=decode(b);
				if(entry==-1)return(-1);
				t[i]=entry*dim;
			}
			for(i=0,o=0;i<dim;i++,o+=step)
			{
				for(j=0;j<step;j++)
				{
					a[offset+o+j]+=valuelist[t[j]+i];
				}
			}

			return(0);
		}

		internal int decodev_add(float[]a, int offset, csBuffer b,int n)
		{
			int i,j,k,entry;
			int t;

			if(dim>8)
			{
				for(i=0;i<n;)
				{
					entry = decode(b);
					if(entry==-1)return(-1);
					t=entry*dim;
					for(j=0;j<dim;)
					{
						a[offset+(i++)]+=valuelist[t+(j++)];
					}
				}
			}
			else
			{
				for(i=0;i<n;)
				{
					entry=decode(b);
					if(entry==-1)return(-1);
					t=entry*dim;
					j=0;
					for(k=0; k < dim; k++)
					{
						a[offset+(i++)]+=valuelist[t+(j++)];
					}
				}
			}    
			return(0);
		}

		internal int decodev_set(float[] a,int offset, csBuffer b, int n)
		{
			int i,j,entry;
			int t;

			for(i=0;i<n;)
			{
				entry = decode(b);
				if(entry==-1)return(-1);
				t=entry*dim;
				for(j=0;j<dim;)
				{
					a[offset+i++]=valuelist[t+(j++)];
				}
			}
			return(0);
		}

		internal int decodevv_add(float[][] a, int offset,int ch, csBuffer b,int n)
		{
			int i,j,entry;
			int chptr=0;
			//System.out.println("decodevv_add: a="+a+",b="+b+",valuelist="+valuelist);

			for(i=offset/ch;i<(offset+n)/ch;)
			{
				entry = decode(b);
				if(entry==-1)return(-1);

				int t = entry*dim;
				for(j=0;j<dim;j++)
				{
					a[chptr][i]+=valuelist[t+j];
					chptr++;
					if(chptr==ch)
					{
						chptr=0;
						i++;
					}
				}
			}
			return(0);
		}


		// Decode side is specced and easier, because we don't need to find
		// matches using different criteria; we simply read and map.  There are
		// two things we need to do 'depending':
		//   
		// We may need to support interleave.  We don't really, but it's
		// convenient to do it here rather than rebuild the vector later.
		//
		// Cascades may be additive or multiplicitive; this is not inherent in
		// the codebook, but set in the code using the codebook.  Like
		// interleaving, it's easiest to do it here.  
		// stage==0 -> declarative (set the value)
		// stage==1 -> additive
		// stage==2 -> multiplicitive

		// returns the entry number or -1 on eof
		internal int decode(csBuffer b)
		{
			int ptr=0;
			DecodeAux t=decode_tree;
			int lok=b.look(t.tabn);
			//System.err.println(this+" "+t+" lok="+lok+", tabn="+t.tabn);

			if(lok>=0)
			{
				ptr=t.tab[lok];
				b.adv(t.tabl[lok]);
				if(ptr<=0)
				{
					return -ptr;
				}
			}
			do
			{
				switch(b.read1())
				{
					case 0:
						ptr=t.ptr0[ptr];
						break;
					case 1:
						ptr=t.ptr1[ptr];
						break;
					case -1:
					default:
						return(-1);
				}
			}
			while(ptr>0);
			return(-ptr);
		}

		// returns the entry number or -1 on eof
		internal int decodevs(float[] a, int index, csBuffer b, int step,int addmul)
		{
			int entry=decode(b);
			if(entry==-1)return(-1);
			switch(addmul)
			{
				case -1:
					for(int i=0,o=0;i<dim;i++,o+=step)
						a[index+o]=valuelist[entry*dim+i];
					break;
				case 0:
					for(int i=0,o=0;i<dim;i++,o+=step)
						a[index+o]+=valuelist[entry*dim+i];
					break;
				case 1:
					for(int i=0,o=0;i<dim;i++,o+=step)
						a[index+o]*=valuelist[entry*dim+i];
					break;
				default:
				//nothing
					break;
			}
			return(entry);
		}

		internal int best(float[] a, int step)
		{
			EncodeAuxNearestMatch nt=c.nearest_tree;
			EncodeAuxThreshMatch tt=c.thresh_tree;
			int ptr=0;

			// we assume for now that a thresh tree is the only other possibility
			if(tt!=null)
			{
				int index=0;
				// find the quant val of each scalar
				for(int k=0,o=step*(dim-1);k<dim;k++,o-=step)
				{
					int i;
					// linear search the quant list for now; it's small and although
					// with > 8 entries, it would be faster to bisect, this would be
					// a misplaced optimization for now
					for(i=0;i<tt.threshvals-1;i++)
					{
						if(a[o]<tt.quantthresh[i])
						{
							break;
						}
					}
					index=(index*tt.quantvals)+tt.quantmap[i];
				}
				// regular lattices are easy :-)
				if(c.lengthlist[index]>0)
				{
					// is this unused?  If so, we'll
					// use a decision tree after all
					// and fall through
					return(index);
				}
			}
			if(nt!=null)
			{
				// optimized using the decision tree
				while(true)
				{
					double cc=0.0;
					int p=nt.p[ptr];
					int q=nt.q[ptr];
					for(int k=0,o=0;k<dim;k++,o+=step)
					{
						cc+=(valuelist[p+k]-valuelist[q+k])*
							(a[o]-(valuelist[p+k]+valuelist[q+k])*.5);
					}
					if(cc>0.0)
					{ // in A
						ptr= -nt.ptr0[ptr];
					}
					else
					{     // in B
						ptr= -nt.ptr1[ptr];
					}
					if(ptr<=0)break;
				}
				return(-ptr);
			}

			// brute force it!
		{
			int besti=-1;
			float best=0.0f;
			int e=0;
			for(int i=0;i<entries;i++)
			{
				if(c.lengthlist[i]>0)
				{
					float _this=dist(dim, valuelist, e, a, step);
					if(besti==-1 || _this<best)
					{
						best=_this;
						besti=i;
					}
				}
				e+=dim;
			}
			return(besti);
		}
		}

		// returns the entry number and *modifies a* to the remainder value
		internal int besterror(float[] a, int step, int addmul)
		{
			int bestt=best(a,step);
			switch(addmul)
			{
				case 0:
					for(int i=0,o=0;i<dim;i++,o+=step)
						a[o]-=valuelist[bestt*dim+i];
					break;
				case 1:
					for(int i=0,o=0;i<dim;i++,o+=step)
					{
						float val=valuelist[bestt*dim+i];
						if(val==0)
						{
							a[o]=0;
						}
						else
						{
							a[o]/=val;
						}
					}
					break;
			}
			return(bestt);
		}

		internal void clear()
		{
			// static book is not cleared; we're likely called on the lookup and
			// the static codebook belongs to the info struct
			//if(decode_tree!=null){
			//  free(b->decode_tree->ptr0);
			//  free(b->decode_tree->ptr1);
			//  memset(b->decode_tree,0,sizeof(decode_aux));
			//  free(b->decode_tree);
			//}
			//if(valuelist!=null)free(b->valuelist);
			//if(codelist!=null)free(b->codelist);
			//memset(b,0,sizeof(codebook));
		}

		internal static float dist(int el, float[] rref, int index, float[] b, int step)
		{
			float acc=(float)0.0;
			for(int i=0; i<el; i++)
			{
				float val=(rref[index+i]-b[i*step]);
				acc+=val*val;
			}
			return(acc);
		}

		/*
		  int init_encode(StaticCodeBook s){
			//memset(c,0,sizeof(codebook));
			c=s;
			entries=s.entries;
			dim=s.dim;
			codelist=make_words(s.lengthlist, s.entries);
			valuelist=s.unquantize();
			return(0);
		  }
		*/

		internal int init_decode(StaticCodeBook s)
		{
			//memset(c,0,sizeof(codebook));
			c=s;
			entries=s.entries;
			dim=s.dim;
			valuelist=s.unquantize();

			decode_tree=make_decode_tree();
			if(decode_tree==null)
			{
				//goto err_out;
				clear();
				return(-1);
			}
			return(0);
			//  err_out:
			//    vorbis_book_clear(c);
			//    return(-1);
		}

		// given a list of word lengths, generate a list of codewords.  Works
		// for length ordered or unordered, always assigns the lowest valued
		// codewords first.  Extended to handle unused entries (length 0)
		internal static int[] make_words(int[] l, int n)
		{
			int[] marker=new int[33];
			int[] r=new int[n];
			//memset(marker,0,sizeof(marker));

			for(int i=0;i<n;i++)
			{
				int length=l[i];
				if(length>0)
				{
					int entry=marker[length];
      
					// when we claim a node for an entry, we also claim the nodes
					// below it (pruning off the imagined tree that may have dangled
					// from it) as well as blocking the use of any nodes directly
					// above for leaves
      
					// update ourself
					if(length<32 && ((uint)entry>>length)!=0)
					{
						// error condition; the lengths must specify an overpopulated tree
						//free(r);
						return(null);
					}
					r[i]=entry;
    
					// Look to see if the next shorter marker points to the node
					// above. if so, update it and repeat.
				{
					for(int j=length;j>0;j--)
					{
						if((marker[j]&1)!=0)
						{
							// have to jump branches
							if(j==1)marker[1]++;
							else marker[j]=marker[j-1]<<1;
							break; // invariant says next upper marker would already
							// have been moved if it was on the same path
						}
						marker[j]++;
					}
				}
      
					// prune the tree; the implicit invariant says all the longer
					// markers were dangling from our just-taken node.  Dangle them
					// from our *new* node.
					for(int j=length+1;j<33;j++)
					{
						if(((uint)marker[j]>>1) == entry)
						{
							entry=marker[j];
							marker[j]=marker[j-1]<<1;
						}
						else
						{
							break;
						}
					}    
				}
			}

			// bitreverse the words because our bitwise packer/unpacker is LSb
			// endian
			for(int i=0;i<n;i++)
			{
				int temp=0;
				for(int j=0;j<l[i];j++)
				{
					temp<<=1;
					temp = (int)((uint)temp | ((uint)r[i]>>j)&1);
				}
				r[i]=temp;
			}

			return(r);
		}

		// build the decode helper tree from the codewords 
		internal DecodeAux make_decode_tree()
		{
			int top=0;
			DecodeAux t=new DecodeAux();
			int[] ptr0=t.ptr0=new int[entries*2];
			int[] ptr1=t.ptr1=new int[entries*2];
			int[] codelist=make_words(c.lengthlist, c.entries);

			if(codelist==null)return(null);
			t.aux=entries*2;

			for(int i=0;i<entries;i++)
			{
				if(c.lengthlist[i]>0)
				{
					int ptr=0;
					int j;
					for(j=0;j<c.lengthlist[i]-1;j++)
					{
						int bit=(int)(((uint)codelist[i]>>j)&1);
						if(bit==0)
						{
							if(ptr0[ptr]==0)
							{
								ptr0[ptr]=++top;
							}
							ptr=ptr0[ptr];
						}
						else
						{
							if(ptr1[ptr]==0)
							{
								ptr1[ptr]= ++top;
							}
							ptr=ptr1[ptr];
						}
					}

					if((((uint)codelist[i]>>j)&1)==0){ ptr0[ptr]=-i; }
					else{ ptr1[ptr]=-i; }

				}
			}
			//free(codelist);

			t.tabn = ilog(entries)-4;

			if(t.tabn<5)t.tabn=5;
			int n = 1<<t.tabn;
			t.tab = new int[n];
			t.tabl = new int[n];
			for(int i = 0; i < n; i++)
			{
				int p = 0;
				int j=0;
				for(j = 0; j < t.tabn && (p > 0 || j == 0); j++)
				{
					if ((i&(1<<j))!=0)
					{
						p = ptr1[p];
					}
					else
					{
						p = ptr0[p];
					}
				}
				t.tab[i]=p;  // -code
				t.tabl[i]=j; // length 
			}

			return(t);
		}

		internal static int ilog(int v)
		{
			int ret=0;
			while(v!=0)
			{
				ret++;
				v = (int)((uint)v >> 1);
			}
			return(ret);
		}

	}

	class DecodeAux
	{
		internal int[] tab;
		internal int[] tabl;
		internal int tabn;

		internal int[] ptr0;
		internal int[] ptr1;
		internal int   aux;        // number of tree entries
	}
}
